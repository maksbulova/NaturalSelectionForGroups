using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class food : MonoBehaviour
{

    
    public int foodEnergy;  // поживна користність їжі


    public creature.FoodType foodType;

    void Start()
    {
        transform.SetParent(GameObject.Find("FoodList").transform);
        Resize();
    }


    public void EatMe(int amount, creature eater)
    {
        if (foodEnergy > amount)
        {
            eater.energy += amount;
            foodEnergy -= amount;
        }
        else
        {
            eater.energy += foodEnergy;
            this.foodEnergy = 0;
        }

        if (this.foodEnergy <= 0)
            Destroy(gameObject);

        Resize();
    }

    public void EatMe(creature eater)
    {
        EatMe(foodEnergy, eater); // целиком
    }

    private void Resize()
    {
        // gameObject.transform.localScale = Vector3.one * (0.2f * energy + 0.8f);
    }
}
