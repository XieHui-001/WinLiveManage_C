//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class IHarrisCornerDetectionLayer : ILayer {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;

  internal IHarrisCornerDetectionLayer(global::System.IntPtr cPtr, bool cMemoryOwn) : base(AoceWrapperPINVOKE.IHarrisCornerDetectionLayer_SWIGUpcast(cPtr), cMemoryOwn) {
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(IHarrisCornerDetectionLayer obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  protected override void Dispose(bool disposing) {
    lock(this) {
      if (swigCPtr.Handle != global::System.IntPtr.Zero) {
        if (swigCMemOwn) {
          swigCMemOwn = false;
          AoceWrapperPINVOKE.delete_IHarrisCornerDetectionLayer(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
      base.Dispose(disposing);
    }
  }

  public void updateParamet(HarrisCornerDetectionParamet t) {
    AoceWrapperPINVOKE.IHarrisCornerDetectionLayer_updateParamet(swigCPtr, HarrisCornerDetectionParamet.getCPtr(t));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
  }

  public HarrisCornerDetectionParamet getParamet() {
    HarrisCornerDetectionParamet ret = new HarrisCornerDetectionParamet(AoceWrapperPINVOKE.IHarrisCornerDetectionLayer_getParamet(swigCPtr), true);
    return ret;
  }

}
