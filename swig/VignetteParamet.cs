//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class VignetteParamet : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal VignetteParamet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(VignetteParamet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~VignetteParamet() {
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
          AoceWrapperPINVOKE.delete_VignetteParamet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public vec2 vignetteCenter {
    set {
      AoceWrapperPINVOKE.VignetteParamet_vignetteCenter_set(swigCPtr, vec2.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = AoceWrapperPINVOKE.VignetteParamet_vignetteCenter_get(swigCPtr);
      vec2 ret = (cPtr == global::System.IntPtr.Zero) ? null : new vec2(cPtr, false);
      return ret;
    } 
  }

  public vec3 vignetteColor {
    set {
      AoceWrapperPINVOKE.VignetteParamet_vignetteColor_set(swigCPtr, vec3.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = AoceWrapperPINVOKE.VignetteParamet_vignetteColor_get(swigCPtr);
      vec3 ret = (cPtr == global::System.IntPtr.Zero) ? null : new vec3(cPtr, false);
      return ret;
    } 
  }

  public float vignetteStart {
    set {
      AoceWrapperPINVOKE.VignetteParamet_vignetteStart_set(swigCPtr, value);
    } 
    get {
      float ret = AoceWrapperPINVOKE.VignetteParamet_vignetteStart_get(swigCPtr);
      return ret;
    } 
  }

  public float vignetteEnd {
    set {
      AoceWrapperPINVOKE.VignetteParamet_vignetteEnd_set(swigCPtr, value);
    } 
    get {
      float ret = AoceWrapperPINVOKE.VignetteParamet_vignetteEnd_get(swigCPtr);
      return ret;
    } 
  }

  public VignetteParamet() : this(AoceWrapperPINVOKE.new_VignetteParamet(), true) {
  }

}
