using PinJuke.Configuration;
using PinJuke.Service.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Service
{
    /// <summary>
    /// See https://stackoverflow.com/a/1344255/628696
    /// </summary>
    public static class KeyGenerator
    {
        private static readonly char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        public static string GetUniqueKey(int size)
        {
            byte[] data = new byte[4 * size];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }
            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;
                result.Append(chars[idx]);
            }
            return result.ToString();
        }
    }

    public record InstallIdPair
    {
        public string PrivateId { get; }
        public string PublicId { get; }

        public static InstallIdPair Generate()
        {
            return new InstallIdPair(KeyGenerator.GetUniqueKey(FirestoreService.ID_LENGTH), KeyGenerator.GetUniqueKey(FirestoreService.ID_LENGTH));
        }

        public InstallIdPair(string privateId, string publicId)
        {
            if (privateId.Length != FirestoreService.ID_LENGTH)
            {
                throw new ArgumentException($"privateId must be {FirestoreService.ID_LENGTH} characters long.", "privateId");
            }
            if (publicId.Length != FirestoreService.ID_LENGTH)
            {
                throw new ArgumentException($"publicId must be {FirestoreService.ID_LENGTH} characters long.", "publicId");
            }
            PrivateId = privateId;
            PublicId = publicId;
        }
    }

    public class BeaconException : Exception
    {
        public BeaconException(string? message) : base(message)
        {
        }
    }

    public record Beacon(
        string AppName,
        string AppVersion,
        string AppFileVersion,
        string Locale,
        string Timezone,
        BeaconDisplay PlayField,
        BeaconDisplay BackGlass,
        BeaconDisplay Dmd,
        BeaconDisplay Topper,
        bool DofEnabled,
        string[] ControllerNames,
        long RamInstalledGigaBytes,
        string? DeveloperName
    );

    public record BeaconDisplay(
        bool Enabled,
        int Left,
        int Top,
        int Width,
        int Height
    );

    public class BeaconService
    {
        private readonly FirestoreService firestoreService = new("pinjuke-music-player", "(default)", "\u0041\u0049\u007A\u0061\u0053\u0079\u0042\u0072\u0044\u0078\u0071\u0051\u0033\u0056\u004A\u0075\u006B\u0048\u0059\u006E\u004F\u006A\u0039\u0054\u0061\u0031\u0073\u0031\u0062\u0065\u0041\u004F\u0063\u004D\u004B\u0074\u006A\u0056\u0051");

        public async Task SendBeacon(InstallIdPair installIdPair, Beacon beacon)
        {
            var beaconId = KeyGenerator.GetUniqueKey(FirestoreService.ID_LENGTH);

            var install = await firestoreService.Find("installs", installIdPair.PrivateId);
            if (install == null)
            {
                if (await IsRateLimited(installIdPair))
                {
                    return;
                }

                await firestoreService.Insert(new Document("installs", installIdPair.PrivateId)
                {
                    ["createdAt"] = new ServerTimestampValue(),
                    ["updatedAt"] = new ServerTimestampValue(),
                    ["publicInstallId"] = new StringValue(installIdPair.PublicId),
                    ["lastBeaconId"] = new StringValue(beaconId),
                });
                await firestoreService.Insert(new Document("publicInstalls", installIdPair.PublicId)
                {
                    ["installId"] = new StringValue(installIdPair.PrivateId),
                });
            }
            else
            {
                await firestoreService.Patch(new Document("installs", installIdPair.PrivateId)
                {
                    ["updatedAt"] = new ServerTimestampValue(),
                    ["lastBeaconId"] = new StringValue(beaconId),
                });
            }

            var document = new Document("beacons", beaconId)
            {
                ["createdAt"] = new ServerTimestampValue(),
                ["publicInstallId"] = new StringValue(installIdPair.PublicId),
                ["appName"] = new StringValue(beacon.AppName),
                ["appVersion"] = new StringValue(beacon.AppVersion),
                ["appFileVersion"] = new StringValue(beacon.AppFileVersion),
                ["locale"] = new StringValue(beacon.Locale),
                ["timezone"] = new StringValue(beacon.Timezone),
                ["playField"] = BeaconDisplayToMapValue(beacon.PlayField),
                ["backGlass"] = BeaconDisplayToMapValue(beacon.BackGlass),
                ["dmd"] = BeaconDisplayToMapValue(beacon.Dmd),
                ["topper"] = BeaconDisplayToMapValue(beacon.Topper),
                ["dofEnabled"] = new BooleanValue(beacon.DofEnabled),
                ["controllerNames"] = new ArrayValue(beacon.ControllerNames.Select(str => new StringValue(str)).ToArray()),
                ["ramInstalledGigaBytes"] = new IntegerValue(beacon.RamInstalledGigaBytes),
            };
            if (beacon.DeveloperName != null)
            {
                document["developerName"] = new StringValue(beacon.DeveloperName);
            }
            await firestoreService.Insert(document);
        }

        private MapValue BeaconDisplayToMapValue(BeaconDisplay display)
        {
            return new MapValue(new OrderedDictionary<string, StaticValue>
            {
                ["enabled"] = new BooleanValue(display.Enabled),
                ["left"] = new IntegerValue(display.Left),
                ["top"] = new IntegerValue(display.Top),
                ["width"] = new IntegerValue(display.Width),
                ["height"] = new IntegerValue(display.Height),
            });
        }

        private async Task<bool> IsRateLimited(InstallIdPair installIdPair)
        {
            await firestoreService.Patch(new Document("rateLimiting", "installs")
            {
                ["currentTime"] = new ServerTimestampValue(),
            });

            var rateLimiting = await firestoreService.Find("rateLimiting", "installs");
            if (rateLimiting == null)
            {
                throw new BeaconException("No rate limiting found for installs.");
            }

            var currentTime = ((TimestampValue)rateLimiting["currentTime"]).Value;
            var limitToNumber = ((IntegerValue)rateLimiting["limitToNumber"]).Value;
            var timeSpanSecs = ((IntegerValue)rateLimiting["timeSpanSecs"]).Value;
            var timeQueue = ((ArrayValue)rateLimiting["timeQueue"]).Value;
            var publicIdQueue = ((ArrayValue)rateLimiting["publicIdQueue"]).Value;
            var shift = timeQueue.Length >= 1
                && (currentTime - ((TimestampValue)timeQueue[0]).Value).TotalSeconds >= timeSpanSecs ? 1 : 0;

            if (timeQueue.Length - shift >= limitToNumber)
            {
                return true;
            }

            await firestoreService.Patch(new Document("rateLimiting", "installs")
            {
                ["timeQueue"] = new ArrayValue([.. timeQueue.Skip(shift), new TimestampValue(currentTime)]),
                ["publicIdQueue"] = new ArrayValue([.. publicIdQueue.Skip(shift), new StringValue(installIdPair.PublicId)]),
            });

            return false;
        }
    }
}
