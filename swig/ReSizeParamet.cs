//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class ReSizeParamet : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal ReSizeParamet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(ReSizeParamet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~ReSizeParamet() {
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
          AoceWrapperPINVOKE.delete_ReSizeParamet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public int bLinear {
    set {
      AoceWrapperPINVOKE.ReSizeParamet_bLinear_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.ReSizeParamet_bLinear_get(swigCPtr);
      return ret;
    } 
  }

  public int newWidth {
    set {
      AoceWrapperPINVOKE.ReSizeParamet_newWidth_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.ReSizeParamet_newWidth_get(swigCPtr);
      return ret;
    } 
  }

  public int newHeight {
    set {
      AoceWrapperPINVOKE.ReSizeParamet_newHeight_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.ReSizeParamet_newHeight_get(swigCPtr);
      return ret;
    } 
  }

  public ReSizeParamet() : this(AoceWrapperPINVOKE.new_ReSizeParamet(), true) {
  }

}
