//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class HighlightShadowTintParamet : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal HighlightShadowTintParamet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(HighlightShadowTintParamet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~HighlightShadowTintParamet() {
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
          AoceWrapperPINVOKE.delete_HighlightShadowTintParamet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public float shadowTintIntensity {
    set {
      AoceWrapperPINVOKE.HighlightShadowTintParamet_shadowTintIntensity_set(swigCPtr, value);
    } 
    get {
      float ret = AoceWrapperPINVOKE.HighlightShadowTintParamet_shadowTintIntensity_get(swigCPtr);
      return ret;
    } 
  }

  public vec3 shadowTintColor {
    set {
      AoceWrapperPINVOKE.HighlightShadowTintParamet_shadowTintColor_set(swigCPtr, vec3.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = AoceWrapperPINVOKE.HighlightShadowTintParamet_shadowTintColor_get(swigCPtr);
      vec3 ret = (cPtr == global::System.IntPtr.Zero) ? null : new vec3(cPtr, false);
      return ret;
    } 
  }

  public float highlightTintIntensity {
    set {
      AoceWrapperPINVOKE.HighlightShadowTintParamet_highlightTintIntensity_set(swigCPtr, value);
    } 
    get {
      float ret = AoceWrapperPINVOKE.HighlightShadowTintParamet_highlightTintIntensity_get(swigCPtr);
      return ret;
    } 
  }

  public vec3 highlightTintColor {
    set {
      AoceWrapperPINVOKE.HighlightShadowTintParamet_highlightTintColor_set(swigCPtr, vec3.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = AoceWrapperPINVOKE.HighlightShadowTintParamet_highlightTintColor_get(swigCPtr);
      vec3 ret = (cPtr == global::System.IntPtr.Zero) ? null : new vec3(cPtr, false);
      return ret;
    } 
  }

  public HighlightShadowTintParamet() : this(AoceWrapperPINVOKE.new_HighlightShadowTintParamet(), true) {
  }

}
