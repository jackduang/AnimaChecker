using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Hotfix
{
    public class MonoBase
    {
        public GameObject gameObject;

        public virtual void Awake() { }

        public virtual void Start() { }

        public virtual void Update() { }

        public virtual void OnDestroy() { }
    }
}
