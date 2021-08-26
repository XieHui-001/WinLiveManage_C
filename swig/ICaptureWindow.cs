//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class ICaptureWindow : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal ICaptureWindow(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(ICaptureWindow obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~ICaptureWindow() {
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
          AoceWrapperPINVOKE.delete_ICaptureWindow(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public virtual void setObserver(ICaptureObserver observer) {
    AoceWrapperPINVOKE.ICaptureWindow_setObserver(swigCPtr, ICaptureObserver.getCPtr(observer));
  }

  public virtual bool bCapturing() {
    bool ret = AoceWrapperPINVOKE.ICaptureWindow_bCapturing(swigCPtr);
    return ret;
  }

  public virtual bool startCapture(IWindow window, bool bSync) {
    bool ret = AoceWrapperPINVOKE.ICaptureWindow_startCapture(swigCPtr, IWindow.getCPtr(window), bSync);
    return ret;
  }

  public virtual bool renderCapture() {
    bool ret = AoceWrapperPINVOKE.ICaptureWindow_renderCapture(swigCPtr);
    return ret;
  }

  public virtual void stopCapture() {
    AoceWrapperPINVOKE.ICaptureWindow_stopCapture(swigCPtr);
  }

}
