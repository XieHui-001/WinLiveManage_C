//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class InputParamet : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal InputParamet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(InputParamet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~InputParamet() {
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
          AoceWrapperPINVOKE.delete_InputParamet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public int bCpu {
    set {
      AoceWrapperPINVOKE.InputParamet_bCpu_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.InputParamet_bCpu_get(swigCPtr);
      return ret;
    } 
  }

  public int bGpu {
    set {
      AoceWrapperPINVOKE.InputParamet_bGpu_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.InputParamet_bGpu_get(swigCPtr);
      return ret;
    } 
  }

  public InputParamet() : this(AoceWrapperPINVOKE.new_InputParamet(), true) {
  }

}
