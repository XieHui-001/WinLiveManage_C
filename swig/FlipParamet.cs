//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class FlipParamet : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal FlipParamet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(FlipParamet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~FlipParamet() {
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
          AoceWrapperPINVOKE.delete_FlipParamet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public int bFlipX {
    set {
      AoceWrapperPINVOKE.FlipParamet_bFlipX_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.FlipParamet_bFlipX_get(swigCPtr);
      return ret;
    } 
  }

  public int bFlipY {
    set {
      AoceWrapperPINVOKE.FlipParamet_bFlipY_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.FlipParamet_bFlipY_get(swigCPtr);
      return ret;
    } 
  }

  public FlipParamet() : this(AoceWrapperPINVOKE.new_FlipParamet(), true) {
  }

}
