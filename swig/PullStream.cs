//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class PullStream : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal PullStream(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(PullStream obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~PullStream() {
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
          AoceWrapperPINVOKE.delete_PullStream(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public int userId {
    set {
      AoceWrapperPINVOKE.PullStream_userId_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.PullStream_userId_get(swigCPtr);
      return ret;
    } 
  }

  public int streamId {
    set {
      AoceWrapperPINVOKE.PullStream_streamId_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.PullStream_streamId_get(swigCPtr);
      return ret;
    } 
  }

  public int bOpen {
    set {
      AoceWrapperPINVOKE.PullStream_bOpen_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.PullStream_bOpen_get(swigCPtr);
      return ret;
    } 
  }

  public PullSetting setting {
    set {
      AoceWrapperPINVOKE.PullStream_setting_set(swigCPtr, PullSetting.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = AoceWrapperPINVOKE.PullStream_setting_get(swigCPtr);
      PullSetting ret = (cPtr == global::System.IntPtr.Zero) ? null : new PullSetting(cPtr, false);
      return ret;
    } 
  }

  public PullStream() : this(AoceWrapperPINVOKE.new_PullStream(), true) {
  }

}
