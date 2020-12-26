using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class creature_view : MonoBehaviour
{
    private creature thisCreature;   // істота чие це поле зору
    void Awake()
    {
        thisCreature = gameObject.GetComponentInParent<creature>();
    }

    /*
     усе що попадає у поле зору істоти класифікується як інша істота або іжа. як тільки воно виходить з поля зору - ця річ забувається.
    їжу істота побачить тільки якщо вона відповідного типу, трава або м'ясо.
    TODO додати всеядних
    */

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log("trigger поля зрения");
        thisCreature.Meet(collision.gameObject);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        thisCreature.UnMeet(collision.gameObject);
    }

}
