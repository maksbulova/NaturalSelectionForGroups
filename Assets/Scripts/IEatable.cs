using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEatable
{
    int FoodEnergy { get; set; }  // поживна користність їжі

    void EatMe(int amount, creature eater);

}
