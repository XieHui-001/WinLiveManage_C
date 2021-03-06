//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class AudioFrame : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal AudioFrame(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(AudioFrame obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~AudioFrame() {
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
          AoceWrapperPINVOKE.delete_AudioFrame(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public int channel {
    set {
      AoceWrapperPINVOKE.AudioFrame_channel_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.AudioFrame_channel_get(swigCPtr);
      return ret;
    } 
  }

  public int sampleRate {
    set {
      AoceWrapperPINVOKE.AudioFrame_sampleRate_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.AudioFrame_sampleRate_get(swigCPtr);
      return ret;
    } 
  }

  public int depth {
    set {
      AoceWrapperPINVOKE.AudioFrame_depth_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.AudioFrame_depth_get(swigCPtr);
      return ret;
    } 
  }

  public long timeStamp {
    set {
      AoceWrapperPINVOKE.AudioFrame_timeStamp_set(swigCPtr, value);
    } 
    get {
      long ret = AoceWrapperPINVOKE.AudioFrame_timeStamp_get(swigCPtr);
      return ret;
    } 
  }

  public int dataSize {
    set {
      AoceWrapperPINVOKE.AudioFrame_dataSize_set(swigCPtr, value);
    } 
    get {
      int ret = AoceWrapperPINVOKE.AudioFrame_dataSize_get(swigCPtr);
      return ret;
    } 
  }

  public SWIGTYPE_p_p_unsigned_char data {
    set {
      AoceWrapperPINVOKE.AudioFrame_data_set(swigCPtr, SWIGTYPE_p_p_unsigned_char.getCPtr(value));
    } 
    get {
      global::System.IntPtr cPtr = AoceWrapperPINVOKE.AudioFrame_data_get(swigCPtr);
      SWIGTYPE_p_p_unsigned_char ret = (cPtr == global::System.IntPtr.Zero) ? null : new SWIGTYPE_p_p_unsigned_char(cPtr, false);
      return ret;
    } 
  }

  public AudioFrame() : this(AoceWrapperPINVOKE.new_AudioFrame(), true) {
  }

}
