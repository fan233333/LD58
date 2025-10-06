using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

namespace Kuchinashi
{
    public class ButtonExtension : Button
    {
        private enum EnumExButtonState
        {
            None,
            PointerDown,
            PointerUp,
            LeftClick,
            RightClick,
            DoubleClick,
            PressBegin,
            Press,
            PressEnd,
        }
    
        private EnumExButtonState mButtonState = EnumExButtonState.None;
        private float mPointerDownTime = 0.0f;
        [SerializeField] private float mDoubleClickInterval = 0.2f;
        [SerializeField] private float mPressBeginTime = 0.3f;
        [SerializeField] private float mPressIntervalTime = 0.2f;
        private float mPressCacheTime = 0f;

        private PointerEventData pointerEventData = null;
    
        public Action OnLeftClick { get; set; }
        public Action<PointerEventData> OnRightClick { get; set; }
        public Action OnDoubleClick { get; set; }
        public Action OnPressBegin { get; set; }
        public Action OnPress { get; set; }
        public Action OnPressEnd { get; set; }

        private int mPressedButton = 0;
    
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (!interactable) return;
    
            if (OnDoubleClick != null)
            {
                if (mButtonState == EnumExButtonState.None)
                {
                    mButtonState = EnumExButtonState.PointerDown;
                    mPointerDownTime = Time.time;
                }
                else if (mButtonState == EnumExButtonState.PointerUp)
                {
                    if (Time.time - mPointerDownTime < mDoubleClickInterval)
                    {
                        mButtonState = EnumExButtonState.DoubleClick;
                        return;
                    }
                    else
                    {
                        mButtonState = EnumExButtonState.PointerDown;
                        mPointerDownTime = Time.time;
                    }
                }
            }
    
            if (OnPressBegin != null || OnPress != null || OnPressEnd != null)
            {
                if (mButtonState != EnumExButtonState.DoubleClick)
                {
                    mButtonState = EnumExButtonState.PointerDown;
                    mPointerDownTime = Time.time;
                }
            }
    
            if (OnLeftClick != null && eventData.button == PointerEventData.InputButton.Left)
            {
                mButtonState = EnumExButtonState.PointerDown;
                mPressedButton = 0;
            }
            else if (OnRightClick != null && eventData.button == PointerEventData.InputButton.Right)
            {
                mButtonState = EnumExButtonState.PointerDown;
                mPressedButton = 1;
            }

            pointerEventData = eventData;
        }
    
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
    
            if (OnDoubleClick != null)
            {
                if (mButtonState == EnumExButtonState.PointerDown)
                {
                    mButtonState = EnumExButtonState.PointerUp;
                    return;
                }
                else if (mButtonState == EnumExButtonState.DoubleClick)
                {
                    return;
                }
            }
    
            if (OnPressBegin != null || OnPress != null || OnPressEnd != null)
            {
                if (mButtonState == EnumExButtonState.Press)
                {
                    mButtonState = EnumExButtonState.PressEnd;
                    return;
                }
            }
    
            if (OnLeftClick != null && eventData.button == PointerEventData.InputButton.Left)
            {
                if (mButtonState == EnumExButtonState.PointerDown)
                    mButtonState = EnumExButtonState.PointerUp;
            }
            else if (OnRightClick != null && eventData.button == PointerEventData.InputButton.Right)
            {
                if (mButtonState == EnumExButtonState.PointerDown)
                    mButtonState = EnumExButtonState.PointerUp;
            }

            pointerEventData = eventData;
        }
    
        private void Update()
        {
            ProcessUpdate();
            ResponseButtonState();
        }
    
        private void ProcessUpdate()
        {
            if (OnDoubleClick != null) { }
    
            if (OnPressBegin != null || OnPress != null || OnPressEnd != null)
            {
                if (mButtonState == EnumExButtonState.PointerDown)
                {
                    if (Time.time - mPointerDownTime > mPressBeginTime)
                    {
                        mButtonState = EnumExButtonState.PressBegin;
                        mPressCacheTime = 0f;
                        return;
                    }
                }
            }
    
            if (OnLeftClick != null && mPressedButton == 0)
            {
                if (mButtonState == EnumExButtonState.PointerUp)
                {
                    if (OnDoubleClick != null)
                    {
                        if (Time.time - mPointerDownTime > mDoubleClickInterval)
                            mButtonState = EnumExButtonState.LeftClick;
                    }
                    else
                    {
                        mButtonState = EnumExButtonState.LeftClick;
                    }
                }
            }
            else if (OnRightClick != null && mPressedButton == 1)
            {
                if (mButtonState == EnumExButtonState.PointerUp)
                {
                    if (OnDoubleClick != null)
                    {
                        if (Time.time - mPointerDownTime > mDoubleClickInterval)
                            mButtonState = EnumExButtonState.RightClick;
                    }
                    else
                    {
                        mButtonState = EnumExButtonState.RightClick;
                    }
                }
            }
        }
    
        private void ResponseButtonState()
        {
            switch (mButtonState)
            {
                case EnumExButtonState.None:
                    break;
                case EnumExButtonState.LeftClick:
                    OnLeftClick?.Invoke();
                    mButtonState = EnumExButtonState.None;
                    break;
                case EnumExButtonState.RightClick:
                    OnRightClick?.Invoke(pointerEventData);
                    mButtonState = EnumExButtonState.None;
                    break;
                case EnumExButtonState.DoubleClick:
                    OnDoubleClick?.Invoke();
                    mButtonState = EnumExButtonState.None;
                    break;
                case EnumExButtonState.PressBegin:
                    OnPressBegin?.Invoke();
                    mButtonState = EnumExButtonState.Press;
                    break;
                case EnumExButtonState.Press:
                    {
                        mPressCacheTime += Time.deltaTime;
                        if (mPressCacheTime >= mPressIntervalTime)
                        {
                            mPressCacheTime = mPressCacheTime - mPressIntervalTime;
                            OnPress?.Invoke();
                        }
                        break;
                    }
                case EnumExButtonState.PressEnd:
                    OnPressEnd?.Invoke();
                    mButtonState = EnumExButtonState.None;
                    break;
                default:
                    break;
            }
        }
    }
}