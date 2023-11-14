using DirectOutput;
using PinJuke.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PinJuke.Dof
{
    class DofMediator : IDisposable
    {
        private readonly MainModel mainModel;
        private readonly Configuration.Dof dof;
        private readonly Pinball pinball;
        private bool disposed = false;

        public DofMediator(MainModel mainModel, Configuration.Dof dof)
        {
            this.mainModel = mainModel;
            this.dof = dof;

            pinball = new Pinball();
            pinball.Setup(dof.GlobalConfigFilePath, "", dof.RomName);
            pinball.Init();

            mainModel.InputEvent += MainModel_InputEvent;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            mainModel.InputEvent -= MainModel_InputEvent;
            pinball.Finish();
        }

        private void MainModel_InputEvent(object? sender, InputActionEventArgs e)
        {
            switch (e.InputAction)
            {
                case InputAction.Previous:
                    pinball.ReceiveData('L', 0, 1); // Lamp 0 on
                    pinball.ReceiveData('L', 0, 0); // Lamp 0 off
                    break;
                case InputAction.Next:
                    pinball.ReceiveData('L', 1, 1); // Lamp 1 on
                    pinball.ReceiveData('L', 1, 0); // Lamp 1 off
                    break;
            }
        }

    }
}
