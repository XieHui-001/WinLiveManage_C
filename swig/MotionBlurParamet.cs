//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class MotionBlurParamet : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal MotionBlurParamet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(MotionBlurParamet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~MotionBlurParamet() {
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
          AoceWrapperPINVOKE.delete_MotionBlurParamet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public float blurSize {
    set {
      AoceWrapperPINVOKE.MotionBlurParamet_blurSize_set(swigCPtr, value);
    } 
    get {
      float ret = AoceWrapperPINVOKE.MotionBlurParamet_blurSize_get(swigCPtr);
      return ret;
    } 
  }

  public float blurAngle {
    set {
      AoceWrapperPINVOKE.MotionBlurParamet_blurAngle_set(swigCPtr, value);
    } 
    get {
      float ret = AoceWrapperPINVOKE.MotionBlurParamet_blurAngle_get(swigCPtr);
      return ret;
    } 
  }

  public MotionBlurParamet() : this(AoceWrapperPINVOKE.new_MotionBlurParamet(), true) {
  }

}
