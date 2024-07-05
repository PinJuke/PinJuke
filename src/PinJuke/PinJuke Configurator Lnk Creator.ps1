$WshShell = New-Object -comObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("PinJuke Configurator.lnk")
$Shortcut.TargetPath = "%SystemRoot%\System32\cmd.exe"
$Shortcut.Arguments = "/c start """" ""%CD%\PinJuke.exe"" --configurator"
$Shortcut.IconLocation = "%SystemRoot%\System32\imageres.dll,-94"
$Shortcut.Save()
