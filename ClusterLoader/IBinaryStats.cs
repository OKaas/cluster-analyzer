using System;
namespace Experiments
{
    interface IBinaryStats
    {
        uint P0 { get; }
        uint P1 { get; }
        void Update(bool val);
    }
}
