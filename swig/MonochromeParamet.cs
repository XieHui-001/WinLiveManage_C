//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class MonochromeParamet : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal MonochromeParamet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(MonochromeParamet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~MonochromeParamet() {
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
          AoceWrapperPINVOKE.delete_MonochromeParamet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public float intensity {
    set {
      AoceWrapperPINVOKE.MonochromeParamet_intensity_set(swigCPtr, value);
    } 
    get {
      float ret = AoceWrapperPINVOKE.MonochromeParamet_intensity_get(swigCPtr);
      return ret;
    } 
  }

  public vec3 color {
    set {
      AoceWrapperPINVOKE.MonochromeParamet_color_set(swigCPtr, vec3.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = AoceWrapperPINVOKE.MonochromeParamet_color_get(swigCPtr);
      vec3 ret = (cPtr == global::System.IntPtr.Zero) ? null : new vec3(cPtr, false);
      return ret;
    } 
  }

  public MonochromeParamet() : this(AoceWrapperPINVOKE.new_MonochromeParamet(), true) {
  }

}