//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class MapChannelParamet : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal MapChannelParamet(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(MapChannelParamet obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~MapChannelParamet() {
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
          AoceWrapperPINVOKE.delete_MapChannelParamet(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public int red {
    set {
      AoceWrapperPINVOKE.MapChannelParamet_red_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.MapChannelParamet_red_get(swigCPtr);
      return ret;
    } 
  }

  public int green {
    set {
      AoceWrapperPINVOKE.MapChannelParamet_green_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.MapChannelParamet_green_get(swigCPtr);
      return ret;
    } 
  }

  public int blue {
    set {
      AoceWrapperPINVOKE.MapChannelParamet_blue_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.MapChannelParamet_blue_get(swigCPtr);
      return ret;
    } 
  }

  public int alpha {
    set {
      AoceWrapperPINVOKE.MapChannelParamet_alpha_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.MapChannelParamet_alpha_get(swigCPtr);
      return ret;
    } 
  }

  public MapChannelParamet() : this(AoceWrapperPINVOKE.new_MapChannelParamet(), true) {
  }

}