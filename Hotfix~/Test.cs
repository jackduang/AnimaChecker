using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Hotfix
{
    public class HelloComponent : MonoBase
    {
        public override void Awake()
        {
            Debug.Log(gameObject.name + " HelloComponent::Awake");
        }

        public override void Start()
        {
            Debug.Log(gameObject.name + " HelloComponent::Start");
            gameObject.GetComponent<MonoProxy>().StartCoroutine(Timer());
        }

        public override void Update()
        {

        }

        public override void OnDestroy()
        {
            Debug.Log(gameObject.name + " HelloComponent::OnDestroy ");
        }
        public System.Collections.IEnumerator Timer()
        {
            Debug.Log("start Timer");
            
            yield return new WaitForSeconds(3);
            Debug.Log("start End");

        }
    }
}