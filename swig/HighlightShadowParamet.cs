//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class HighlightShadowParamet : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal HighlightShadowParamet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(HighlightShadowParamet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~HighlightShadowParamet() {
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
          AoceWrapperPINVOKE.delete_HighlightShadowParamet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public float shadows {
    set {
      AoceWrapperPINVOKE.HighlightShadowParamet_shadows_set(swigCPtr, value);
    } 
    get {
      float ret = AoceWrapperPINVOKE.HighlightShadowParamet_shadows_get(swigCPtr);
      return ret;
    } 
  }

  public float highlights {
    set {
      AoceWrapperPINVOKE.HighlightShadowParamet_highlights_set(swigCPtr, value);
    } 
    get {
      float ret = AoceWrapperPINVOKE.HighlightShadowParamet_highlights_get(swigCPtr);
      return ret;
    } 
  }

  public HighlightShadowParamet() : this(AoceWrapperPINVOKE.new_HighlightShadowParamet(), true) {
  }

}
