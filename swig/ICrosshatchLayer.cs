//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class ICrosshatchLayer : ILayer {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal ICrosshatchLayer(global::System.IntPtr cPtr, bool cMemoryOwn) : base(AoceWrapperPINVOKE.ICrosshatchLayer_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(ICrosshatchLayer obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          AoceWrapperPINVOKE.delete_ICrosshatchLayer(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public void updateParamet(CrosshatchParamet t) {
    AoceWrapperPINVOKE.ICrosshatchLayer_updateParamet(swigCPtr, CrosshatchParamet.getCPtr(t));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
  }

  public CrosshatchParamet getParamet() {
    CrosshatchParamet ret = new CrosshatchParamet(AoceWrapperPINVOKE.ICrosshatchLayer_getParamet(swigCPtr), true);
    return ret;
  }

}