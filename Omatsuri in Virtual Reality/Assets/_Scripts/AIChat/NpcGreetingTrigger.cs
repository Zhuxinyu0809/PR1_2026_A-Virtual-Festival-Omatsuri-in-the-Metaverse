/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License.
 *
 * Meta XR SDK v85 AI Blocks Integration - NPC Proximity Greeting
 */

using System.Collections;
using UnityEngine;
using Meta.XR.BuildingBlocks.AIBlocks; // 引入 Meta XR v85 AI Blocks 官方命名空間

namespace Meta.XR.BuildingBlocks.AIBlocks
{
    public class NpcGreetingTrigger : MonoBehaviour
    {
        [Header("TTS 設定")]
        [Tooltip("負責發聲的 TextToSpeechAgent。若留空，腳本會嘗試自動獲取此物件或子物件中的 Agent。")]
        [SerializeField] private TextToSpeechAgent _ttsAgent;

        [Header("感應範圍")]
        [Tooltip("拖入 AI Triggers 物件上的 Box Collider (可位於子物件，例如 AI Triggers)")]
        public BoxCollider greetingCollider;

        [Header("打招呼文本")]
        [TextArea(3, 6)]
        [Tooltip("此 NPC 專屬的打招呼內容。")]
        [SerializeField] private string _greetingText = "哈囉！歡迎來到這個虛擬世界！";

        [Header("觸發設定")]
        [Tooltip("玩家物件的 Tag，用於識別觸發對象。")]
        [SerializeField] private string _playerTag = "Player";

        [Tooltip("當玩家離開感應範圍後，才開始計算的冷卻時間（秒）。")]
        [SerializeField] private float _cooldownDuration = 8f;

        private bool _isPlayerInside = false;
        private bool _inCooldown = false;
        private Coroutine _cooldownCo;

        private void Awake()
        {
            // 1. 自動配對 TextToSpeechAgent
            if (_ttsAgent == null)
            {
                _ttsAgent = GetComponentInChildren<TextToSpeechAgent>();
                if (_ttsAgent == null)
                {
                    Debug.LogError($"[NpcGreeting] 找不到 TextToSpeechAgent。請手動拖拽指派至 {gameObject.name}。");
                }
            }

            // 2. 參考 NpcAIController 嘅設計，動態配置物理轉發器 (Forwarder)
            if (greetingCollider != null)
            {
                ConfigureTriggerCollider(greetingCollider);
            }
            else
            {
                // 若未在 Inspector 指派，自動在自身或子物件尋找 BoxCollider
                greetingCollider = GetComponentInChildren<BoxCollider>();
                if (greetingCollider != null)
                {
                    ConfigureTriggerCollider(greetingCollider);
                }
                else
                {
                    Debug.LogError($"[NpcGreeting] {gameObject.name} 找不到 BoxCollider！請在 Inspector 手動指派 greetingCollider。");
                }
            }
        }

        /// <summary>
        /// 配置 Trigger Collider 並掛載 Forwarder 轉發器
        /// </summary>
        private void ConfigureTriggerCollider(BoxCollider collider)
        {
            collider.isTrigger = true;
            GreetingTriggerForwarder forwarder = collider.GetComponent<GreetingTriggerForwarder>();
            if (forwarder == null)
            {
                forwarder = collider.gameObject.AddComponent<GreetingTriggerForwarder>();
            }
            forwarder.mainController = this;
        }

        // --- 核心物理事件接收處理 (由 Forwarder 轉發) ---

        public void HandleTriggerEnter(Collider other)
        {
            if (other.CompareTag(_playerTag))
            {
                _isPlayerInside = true;
                TryTriggerGreeting();
            }
        }

        public void HandleTriggerExit(Collider other)
        {
            if (other.CompareTag(_playerTag))
            {
                if (_isPlayerInside)
                {
                    _isPlayerInside = false;
                    StartCooldown();
                }
            }
        }

        /// <summary>
        /// 嘗試觸發 NPC 打招呼
        /// </summary>
        public void TryTriggerGreeting()
        {
            if (_ttsAgent == null) return;

            // 如果目前處於冷卻階段，則不進行打招呼
            if (_inCooldown)
            {
                return;
            }

            // 調用 TextToSpeechAgent 的 SpeakText 並傳入專屬文本
            _ttsAgent.SpeakText(_greetingText);
        }

        /// <summary>
        /// 開始執行冷卻計時
        /// </summary>
        private void StartCooldown()
        {
            if (_cooldownCo != null)
            {
                StopCoroutine(_cooldownCo);
            }
            _cooldownCo = StartCoroutine(CoCooldownRoutine());
        }

        private IEnumerator CoCooldownRoutine()
        {
            _inCooldown = true;
            yield return new WaitForSeconds(_cooldownDuration);
            _inCooldown = false;
            _cooldownCo = null;
        }

        #region Public Getters/Setters
        public string GreetingText
        {
            get => _greetingText;
            set => _greetingText = value;
        }

        // 當前是否可以觸發打招呼（玩家不在範圍內，且不在冷卻中）
        public bool CanGreet => !_isPlayerInside && !_inCooldown;

        // 玩家目前是否仍在感應範圍內
        public bool IsPlayerInside => _isPlayerInside;

        // 目前是否正在冷卻中
        public bool InCooldown => _inCooldown;
        #endregion
    }

    /// <summary>
    /// 內部轉發器類別：用於掛載在指定的 BoxCollider 上並接收物理事件，然後轉發回 NpcGreetingTrigger
    /// </summary>
    public class GreetingTriggerForwarder : MonoBehaviour
    {
        public NpcGreetingTrigger mainController;

        private void OnTriggerEnter(Collider other)
        {
            if (mainController != null)
            {
                mainController.HandleTriggerEnter(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (mainController != null)
            {
                mainController.HandleTriggerExit(other);
            }
        }
    }
}