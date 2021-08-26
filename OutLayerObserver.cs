using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AocePackage
{
    class OutLayerObserver : IOutputLayerObserver
    {
        public override void onImageProcess(IntPtr data, ImageFormat imageFormat, int outIndex)
        {
            AoceManager.Instance.pushVideoFrame(data, imageFormat);
        }
    }
}
