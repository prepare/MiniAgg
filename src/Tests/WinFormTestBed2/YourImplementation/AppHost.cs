﻿//Apache2, 2014-present, WinterDev
using System;
using System.IO;
using PixelFarm.Drawing;

namespace LayoutFarm
{
    public abstract class AppHost
    {

        protected int _primaryScreenWorkingAreaW;
        protected int _primaryScreenWorkingAreaH;
       

        public AppHost()
        {
        }
        //override this to get exact executable path
        public virtual string ExecutablePath => System.IO.Directory.GetCurrentDirectory();

        public void StartApp(App app)
        {
            if (PreviewApp(app))
            {
                app.StartApp(this);
            }
        }
        protected virtual bool PreviewApp(App app)
        {
            return true;
        }

         

        public abstract Image LoadImage(string imgName, int reqW, int reqH);
        public abstract Image LoadImage(byte[] rawImgFile, string imgTypeHint);

        public Image LoadImage(string imgName)
        {
            return LoadImage(imgName, 0, 0);
        }

        

        public virtual System.IO.Stream GetReadStream(string src)
        {
            return App.ReadStreamS(src);
        }
        public virtual System.IO.Stream GetWriteStream(string dest)
        {
            return App.GetWriteStream(dest);
        }
        public virtual bool UploadStream(string url, Stream stream)
        {
            return App.UploadStream(url, stream);
        }
        //
        public int PrimaryScreenWidth => _primaryScreenWorkingAreaW;
        public int PrimaryScreenHeight => _primaryScreenWorkingAreaH;
        //
        public abstract void AddChild(RenderElement renderElement);
        public abstract void AddChild(RenderElement renderElement, object owner);

        public abstract RootGraphic RootGfx { get; }

        public virtual ImageBinder LoadImageAndBind(string src)
        {
            ImageBinder clientImgBinder = new ImageBinder(src);
            clientImgBinder.SetLocalImage(LoadImage(src));
            return clientImgBinder;
        }

        public ImageBinder CreateImageBinder(string src)
        {
            ImageBinder clientImgBinder = new ImageBinder(src);
            clientImgBinder.SetImageLoader(binder =>
            {
                Image img = this.LoadImage(binder.ImageSource);
                binder.SetLocalImage(img);
            });
            return clientImgBinder;
        }
        public virtual void CustomContentRequest(object customContentReq) { }
    }

}
