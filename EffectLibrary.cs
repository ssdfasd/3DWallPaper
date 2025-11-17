using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XCWallPaper
{
    // 用户的特效库
    class EffectLibrary
    {
        private static readonly Lazy<EffectLibrary> _instance = new Lazy<EffectLibrary>(() => new EffectLibrary());
        public static EffectLibrary Instance => _instance.Value;


    }
}
