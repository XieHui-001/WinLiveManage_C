//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class IWindow : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal IWindow(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(IWindow obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~IWindow() {
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
          AoceWrapperPINVOKE.delete_IWindow(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public virtual string getTitle() {
    string ret = AoceWrapperPINVOKE.IWindow_getTitle(swigCPtr);
    return ret;
  }

  public virtual global::System.IntPtr getHwnd() {
    global::System.IntPtr ret = AoceWrapperPINVOKE.IWindow_getHwnd(swigCPtr);
    return ret;
  }

  public virtual ulong getProcessId() {
    ulong ret = AoceWrapperPINVOKE.IWindow_getProcessId(swigCPtr);
    return ret;
  }

  public virtual ulong getMainThreadId() {
    ulong ret = AoceWrapperPINVOKE.IWindow_getMainThreadId(swigCPtr);
    return ret;
  }

  public virtual bool bValid() {
    bool ret = AoceWrapperPINVOKE.IWindow_bValid(swigCPtr);
    return ret;
  }

}