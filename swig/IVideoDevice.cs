//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class IVideoDevice : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal IVideoDevice(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(IVideoDevice obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~IVideoDevice() {
    Dispose(false);
  }

  public void Dispose() {
    Dispose(true);
    global::System.GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          AoceWrapperPINVOKE.delete_IVideoDevice(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public virtual void setObserver(IVideoDeviceObserver observer) {
    AoceWrapperPINVOKE.IVideoDevice_setObserver(swigCPtr, IVideoDeviceObserver.getCPtr(observer));
  }

  public virtual int getFormatCount() {
    int ret = AoceWrapperPINVOKE.IVideoDevice_getFormatCount(swigCPtr);
    return ret;
  }

  public virtual VideoFormat getFormat(int index) {
    VideoFormat ret = new VideoFormat(AoceWrapperPINVOKE.IVideoDevice_getFormat(swigCPtr, index), true);
    return ret;
  }

  public virtual string getName() {
    string ret = AoceWrapperPINVOKE.IVideoDevice_getName(swigCPtr);
    return ret;
  }

  public virtual string getId() {
    string ret = AoceWrapperPINVOKE.IVideoDevice_getId(swigCPtr);
    return ret;
  }

  public virtual VideoFormat getSelectFormat() {
    VideoFormat ret = new VideoFormat(AoceWrapperPINVOKE.IVideoDevice_getSelectFormat(swigCPtr), true);
    return ret;
  }

  public virtual bool back() {
    bool ret = AoceWrapperPINVOKE.IVideoDevice_back(swigCPtr);
    return ret;
  }

  public virtual bool bDepth() {
    bool ret = AoceWrapperPINVOKE.IVideoDevice_bDepth(swigCPtr);
    return ret;
  }

  public virtual int findFormatIndex(int width, int height, int fps) {
    int ret = AoceWrapperPINVOKE.IVideoDevice_findFormatIndex__SWIG_0(swigCPtr, width, height, fps);
    return ret;
  }

  public virtual int findFormatIndex(int width, int height) {
    int ret = AoceWrapperPINVOKE.IVideoDevice_findFormatIndex__SWIG_1(swigCPtr, width, height);
    return ret;
  }

  public virtual void setFormat(int index) {
    AoceWrapperPINVOKE.IVideoDevice_setFormat__SWIG_0(swigCPtr, index);
  }

  public virtual void setFormat(VideoFormat format) {
    AoceWrapperPINVOKE.IVideoDevice_setFormat__SWIG_1(swigCPtr, VideoFormat.getCPtr(format));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
  }

  public virtual bool open() {
    bool ret = AoceWrapperPINVOKE.IVideoDevice_open(swigCPtr);
    return ret;
  }

  public virtual bool close() {
    bool ret = AoceWrapperPINVOKE.IVideoDevice_close(swigCPtr);
    return ret;
  }

  public virtual bool bOpen() {
    bool ret = AoceWrapperPINVOKE.IVideoDevice_bOpen(swigCPtr);
    return ret;
  }

}