//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class PerlinNoiseParamet : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal PerlinNoiseParamet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(PerlinNoiseParamet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~PerlinNoiseParamet() {
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
          AoceWrapperPINVOKE.delete_PerlinNoiseParamet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public float scale {
    set {
      AoceWrapperPINVOKE.PerlinNoiseParamet_scale_set(swigCPtr, value);
    } 
    get {
      float ret = AoceWrapperPINVOKE.PerlinNoiseParamet_scale_get(swigCPtr);
      return ret;
    } 
  }

  public vec4 colorStart {
    set {
      AoceWrapperPINVOKE.PerlinNoiseParamet_colorStart_set(swigCPtr, vec4.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = AoceWrapperPINVOKE.PerlinNoiseParamet_colorStart_get(swigCPtr);
      vec4 ret = (cPtr == global::System.IntPtr.Zero) ? null : new vec4(cPtr, false);
      return ret;
    } 
  }

  public vec4 colorFinish {
    set {
      AoceWrapperPINVOKE.PerlinNoiseParamet_colorFinish_set(swigCPtr, vec4.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = AoceWrapperPINVOKE.PerlinNoiseParamet_colorFinish_get(swigCPtr);
      vec4 ret = (cPtr == global::System.IntPtr.Zero) ? null : new vec4(cPtr, false);
      return ret;
    } 
  }

  public PerlinNoiseParamet() : this(AoceWrapperPINVOKE.new_PerlinNoiseParamet(), true) {
  }

}
