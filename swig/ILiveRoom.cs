//------------------------------------------------------------------------------
// <auto-generated />
//
// This file was automatically generated by SWIG (http://www.swig.org).
// Version 4.0.2
//
// Do not make changes to this file unless you know what you are doing--modify
// the SWIG interface file instead.
//------------------------------------------------------------------------------


public class ILiveRoom : global::System.IDisposable {
  private global::System.Runtime.InteropServices.HandleRef swigCPtr;
  protected bool swigCMemOwn;

  internal ILiveRoom(global::System.IntPtr cPtr, bool cMemoryOwn) {
    swigCMemOwn = cMemoryOwn;
    swigCPtr = new global::System.Runtime.InteropServices.HandleRef(this, cPtr);
  }

  internal static global::System.Runtime.InteropServices.HandleRef getCPtr(ILiveRoom obj) {
    return (obj == null) ? new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero) : obj.swigCPtr;
  }

  ~ILiveRoom() {
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
          AoceWrapperPINVOKE.delete_ILiveRoom(swigCPtr);
        }
        swigCPtr = new global::System.Runtime.InteropServices.HandleRef(null, global::System.IntPtr.Zero);
      }
    }
  }

  public virtual int getUserId() {
    int ret = AoceWrapperPINVOKE.ILiveRoom_getUserId(swigCPtr);
    return ret;
  }

  public virtual int getPullIndex(int userId, int index) {
    int ret = AoceWrapperPINVOKE.ILiveRoom_getPullIndex(swigCPtr, userId, index);
    return ret;
  }

  public virtual float getMicVolume() {
    float ret = AoceWrapperPINVOKE.ILiveRoom_getMicVolume(swigCPtr);
    return ret;
  }

  public virtual void setPlayVolume(int value) {
    AoceWrapperPINVOKE.ILiveRoom_setPlayVolume(swigCPtr, value);
  }

  public virtual bool initRoom(global::System.IntPtr liveContext, ILiveObserver liveBack) {
    bool ret = AoceWrapperPINVOKE.ILiveRoom_initRoom(swigCPtr, liveContext, ILiveObserver.getCPtr(liveBack));
    return ret;
  }

  public virtual bool loginRoom(string roomName, int useId, int pushCount) {
    bool ret = AoceWrapperPINVOKE.ILiveRoom_loginRoom(swigCPtr, roomName, useId, pushCount);
    return ret;
  }

  public virtual bool pushStream(int index, PushSetting setting) {
    bool ret = AoceWrapperPINVOKE.ILiveRoom_pushStream(swigCPtr, index, PushSetting.getCPtr(setting));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public virtual void stopPushStream(int index) {
    AoceWrapperPINVOKE.ILiveRoom_stopPushStream(swigCPtr, index);
  }

  public virtual bool pushVideoFrame(int index, VideoFrame videoFrame) {
    bool ret = AoceWrapperPINVOKE.ILiveRoom_pushVideoFrame(swigCPtr, index, VideoFrame.getCPtr(videoFrame));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public virtual bool pushAudioFrame(int index, AudioFrame audioFrame) {
    bool ret = AoceWrapperPINVOKE.ILiveRoom_pushAudioFrame(swigCPtr, index, AudioFrame.getCPtr(audioFrame));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public virtual bool pullStream(int userId, int index, PullSetting setting) {
    bool ret = AoceWrapperPINVOKE.ILiveRoom_pullStream(swigCPtr, userId, index, PullSetting.getCPtr(setting));
    if (AoceWrapperPINVOKE.SWIGPendingException.Pending) throw AoceWrapperPINVOKE.SWIGPendingException.Retrieve();
    return ret;
  }

  public virtual void stopPullStream(int userId, int index) {
    AoceWrapperPINVOKE.ILiveRoom_stopPullStream(swigCPtr, userId, index);
  }

  public virtual void logoutRoom() {
    AoceWrapperPINVOKE.ILiveRoom_logoutRoom(swigCPtr);
  }

  public virtual void shutdownRoom() {
    AoceWrapperPINVOKE.ILiveRoom_shutdownRoom(swigCPtr);
  }

}