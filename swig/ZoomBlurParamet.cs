//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class ZoomBlurParamet : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal ZoomBlurParamet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(ZoomBlurParamet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~ZoomBlurParamet() {
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
          AoceWrapperPINVOKE.delete_ZoomBlurParamet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public vec2 blurCenter {
    set {
      AoceWrapperPINVOKE.ZoomBlurParamet_blurCenter_set(swigCPtr, vec2.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = AoceWrapperPINVOKE.ZoomBlurParamet_blurCenter_get(swigCPtr);
      vec2 ret = (cPtr == global::System.IntPtr.Zero) ? null : new vec2(cPtr, false);
      return ret;
    } 
  }

  public float blurSize {
    set {
      AoceWrapperPINVOKE.ZoomBlurParamet_blurSize_set(swigCPtr, value);
    } 
    get {
      float ret = AoceWrapperPINVOKE.ZoomBlurParamet_blurSize_get(swigCPtr);
      return ret;
    } 
  }

  public ZoomBlurParamet() : this(AoceWrapperPINVOKE.new_ZoomBlurParamet(), true) {
  }

}
