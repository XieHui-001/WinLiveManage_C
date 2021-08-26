//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class IVideoManager : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal IVideoManager(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(IVideoManager obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~IVideoManager() {
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
          AoceWrapperPINVOKE.delete_IVideoManager(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public virtual int getDeviceCount(bool bUpdate) {
    int ret = AoceWrapperPINVOKE.IVideoManager_getDeviceCount__SWIG_0(swigCPtr, bUpdate);
    return ret;
  }

  public virtual int getDeviceCount() {
    int ret = AoceWrapperPINVOKE.IVideoManager_getDeviceCount__SWIG_1(swigCPtr);
    return ret;
  }

  public virtual IVideoDevice getDevice(int index) {
    global::System.IntPtr cPtr = AoceWrapperPINVOKE.IVideoManager_getDevice(swigCPtr, index);
    IVideoDevice ret = (cPtr == global::System.IntPtr.Zero) ? null : new IVideoDevice(cPtr, false);
    return ret;
  }

}
