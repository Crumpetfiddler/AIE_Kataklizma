﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay {

    [RequireComponent(typeof(EntityStats))]
    public class CollapseOnDeath : MonoBehaviour {
        private bool IsFalling { get; set; }


        public bool UseSceneGravity = true;
        public float LocalGravity = 0.05f; // "Fall Speed"
        public float MaxFallSpeed = 1.0f;

        public float TimeToLive = 1; // "Fall Time"

        private Rigidbody Rb;
        private EntityStats Stats;

        /// Tracks velocity on a rigidbody if it exists, else tracks it locally
        private Vector3 _Velocity;
        private Vector3 Velocity {
            get {
                return Rb != null ? Rb.velocity : _Velocity;
            }
            set {
                if (Rb != null)
                    Rb.velocity = value;
                else
                    _Velocity = value;
            }
        }


        // Use this for initialization
        void Start() {
            IsFalling = false;

            Rb = GetComponent<Rigidbody>();
            if (Rb == null) throw new ApplicationException(gameObject.name + " - CollapseOnDeath could not locate required Rigidbody sibling.");

            Stats = GetComponent<EntityStats>();
            if (Stats == null) throw new ApplicationException(gameObject.name + " - CollapseOnDeath could not locate required EntityStats sibling.");

            Stats.ValueChanged += OnStatsValueChanged;
        }

        private void OnStatsValueChanged(UnityEngine.Object sender, ValueType type, ValueSubtype subtype, float old) {
            Debug.Log("Death Check: " + type.ToString() + ", " + subtype.ToString() + ", " + old.ToString("N2") + " -> " + Stats[type, subtype].ToString("N2"));
            if ( type == ValueType.Health && Stats[ValueType.Health] <= 0 && !IsFalling) {
                // Object Died
                IsFalling = true;
                Destroy(gameObject, TimeToLive);
            }
        }


        // Update is called once per frame
        void Update() {
            if (IsFalling == true) {
                Velocity += UseSceneGravity ? Physics.gravity : new Vector3(0, -LocalGravity, 0); // Increase velocity from gravity

                Velocity = Vector3.ClampMagnitude(Velocity, MaxFallSpeed); // Limit to MaxfallSpeed

                if (Rb == null) // If we have a rigidbody then volicty is already updating position.
                    transform.position += Velocity;
            }
        }
    }
}