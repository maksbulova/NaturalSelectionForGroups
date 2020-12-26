using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class creature : MonoBehaviour
{

    public int energy;        // инт от 0 до 100

    [SerializeField] private int speed;         // от 0 до хз
    [SerializeField] private int perception;    // хз
    [SerializeField] private int camouflage;    // сейм щит
    [SerializeField] private int size;          // существо может съесть существо на треть меньше себя
    [SerializeField] private int birthRate;     // от 1 до хз

    public List<creature> creaturesInView;
    public List<creature> threatsInView;
    public List<creature> PreyInView;
    public List<food> foodInView;
    private State state;

    public int starveTime;
    public float birthHunger; // во сколько раз должен быть превышен хангерлвл для рождения ребенка
    private int hungerLvl;

    public int Speed
    {
        get { return speed; }
        set
        {
            if (value < 1 )
            {
                speed = 1;
            }
            else if (value > 20)
            {
                speed = 20;
            }
            else
            {
                speed = value;
            }
        }
    }

    public int Perception
    {
        get { return perception; }
        set
        {
            if (value < 1)
            {
                perception = 1;
            }
            else if (value > 20)
            {
                perception = 20;
            }
            else
            {
                perception = value;
            }
        }
    }

    public int Camouflage
    {
        get { return camouflage; }
        set
        {
            if (value < 1)
            {
                camouflage = 1;
            }
            else if (value > 20)
            {
                camouflage = 20;
            }
            else
            {
                camouflage = value;
            }
        }
    }

    public int Size
    {
        get { return size; }
        set
        {
            if (value < 1)
            {
                size = 1;
            }
            else if (value > 20)
            {
                size = 20;
            }
            else
            {
                size = value;
            }
        }
    }

    public int BirthRate
    {
        get { return birthRate; }
        set
        {
            if (value < 1)
            {
                birthRate = 1;
            }
            else if (value > 20)
            {
                birthRate = 20;
            }
            else
            {
                birthRate = value;
            }
        }
    }





    public enum FoodType
    {
        grass,
        meat
    }
    enum State
    {
        Hungry,
        Calm,
        Panic
    }
    
    public FoodType foodType;


    void Start()
    {
        transform.SetParent(GameObject.Find("CreatureList").transform);
        gameObject.transform.GetChild(0).GetComponent<CircleCollider2D>().radius = perception;  // задає дальність поля зору
        hungerLvl = EnergyFormula() * starveTime; // за 10 секунд до голодной смерти 

        StartCoroutine(EnergyConsumption());

        CheckState();

    }

    private void CheckState()
    {
        if (threatsInView.Count != 0)
        {
            state = State.Panic;
        }
        else if (energy > hungerLvl * birthHunger)
        {
            Birth();
            return;
        }
        else if (energy < hungerLvl)
        {
            state = State.Hungry;
        }
        else
        {
            state = State.Calm;
        }

        Behavior();
    }

    void Update()
    {
        if (Input.GetKeyDown("b"))
        {
            Birth();
        }
    }

    private void StopActions()
    {
        StopAllCoroutines();
        StartCoroutine(EnergyConsumption());
    }

    private void Behavior() // мб лучше через делегат переписать
    {
        switch (state)
        {
            case State.Hungry:
                if (PreyInView.Count > 0) // TODO приоритет мб перенастроить
                {
                    StopActions();
                    StartCoroutine(Hunt(FindClosest<creature>(PreyInView)));
                }
                else if (foodInView.Count > 0)
                {
                    StopActions();
                    StartCoroutine(MoveEat(FindClosest<food>(foodInView)));
                    // StartCoroutine(MoveTo(FindClosest<food>(foodInView).transform.position));
                }
                else
                {
                    StopActions();
                    Wander();
                }
                break;

            case State.Calm:
                StopActions();
                Wander();
                break;

            case State.Panic:
                StopActions();
                StartCoroutine(Escape());
                break;

            default:
                break;
        }
    }


    private enum MeetType
    {
        food,
        threat,
        creature,
        prey,
        unknow
    }

    public void Meet(GameObject obj)  // обработка новых встречь
    {
        MeetType meet;

        meet = MeetType.unknow;

        if (obj.CompareTag("creature"))
        {
            creature anotherCreature = obj.GetComponent<creature>();

            if (anotherCreature.size > this.size * 1.3f)
            {
                meet = MeetType.threat;
            }
            else if (this.size > anotherCreature.size * 1.3f)
            {
                meet = MeetType.prey;
            }
            else
            {
                meet = MeetType.creature;
            }

            // thisCreature.creaturesInView.Add(obj.GetComponent<creature>());
        }
        else if (obj.CompareTag("food"))
        {
            meet = MeetType.food;

            // if (obj.GetComponent<food>().foodType == thisCreature.foodType)         
            //    thisCreature.food_in_view.Add(obj.GetComponent<food>());
        }

        switch (meet)
        {
            case MeetType.food:
                this.foodInView.Add(obj.GetComponent<food>());
                break;
            case MeetType.threat:
                this.threatsInView.Add(obj.GetComponent<creature>());
                break;
            case MeetType.creature:
                this.creaturesInView.Add(obj.GetComponent<creature>());
                break;
            case MeetType.prey:
                this.PreyInView.Add(obj.GetComponent<creature>());
                break;
            case MeetType.unknow:
                return;
            default:
                break;
        }

        CheckState();
    }

    public void UnMeet(GameObject obj)
    {
        threatsInView.Remove(obj.GetComponent<creature>());
        creaturesInView.Remove(obj.GetComponent<creature>());
        foodInView.Remove(obj.GetComponent<food>());
        PreyInView.Remove(obj.GetComponent<creature>());

        CheckState();
    }

    // с шансом chance меняет мараметр с parentParametr на +- размер mutationSize
    private int Mutate(int parentParametr, int mutationSize = 1, int chance = 10)
    {


        if (Random.Range(0, 100) < chance)
        {
            if (Random.value > 0.5f)
            {
                return parentParametr + mutationSize;
            }
            else
            {
                return parentParametr - mutationSize;
            }
        }
        else
        {
            return parentParametr;
        }
    }

    public void Evolve(creature parent)
    {
        this.Speed = Mutate(parent.Speed);
        this.Perception = Mutate(parent.Perception);
        this.Camouflage = Mutate(parent.Camouflage);
        this.Size = Mutate(parent.Size);
        this.BirthRate = Mutate(parent.BirthRate);
    }


    private void Birth()
    {
        creature child;
        for (int i = 0; i < birthRate; i++)
        {
            child = Instantiate(this);
            child.Evolve(this);

            child.energy = (this.energy / 2) / birthRate;
            child.hungerLvl = this.hungerLvl;
        }

        this.energy /= 2;

        CheckState();
    }


    private int EnergyFormula()
    {
        return speed * size + perception + camouflage;
    }

    private IEnumerator EnergyConsumption()     // расход энергии, над формулой надо поработать
    {
        while (true)
        {
            energy -= EnergyFormula();

            /*
            if (energy < hungerLvl) // не супер оптимизировано мб
                CheckState();
            */

            if (energy <= 0)
                Death();

            yield return new WaitForSeconds(1f);
        }
    }


    private IEnumerator MoveEat(food dish)
    {
        Vector2 dir = dish.transform.position - gameObject.transform.position; ;
        while (foodInView.Contains(dish)) // TODO добавить мб чтоб не на истощение преследовал
        {
            MoveStepTo(dir);
            yield return new WaitForFixedUpdate();
        }

        CheckState();
    }



    private IEnumerator Hunt(creature prey)
    {
        Vector2 dir;
        while (PreyInView.Contains(prey)) // TODO добавить мб чтоб не на истощение преследовал
        {
            dir = prey.transform.position - gameObject.transform.position;
            MoveStepTo(dir);
            yield return new WaitForFixedUpdate();
        }

        CheckState();
    }

    private void Wander()
    {
        StartCoroutine(MoveTo(RandomPos()));
    }
    private IEnumerator Escape()
    {
        Vector2 dir;
        Vector2 pos;
        while (threatsInView.Count > 0)
        {
            pos = transform.position;
            dir = pos - FindMiddlePoint(threatsInView);
            MoveStepTo(dir);
            yield return new WaitForFixedUpdate();
        }

        CheckState();
    }

    private float DistanceTo<T> (T obj) where T : MonoBehaviour
    {
        return (this.transform.position - obj.transform.position).magnitude;
    }


    private T FindClosest<T> (List<T> list) where T : MonoBehaviour
    {
        T closestObj = list[0];
        float closestDist = DistanceTo<T>(closestObj);
        foreach (T obj in list)
        {
            if (DistanceTo<T>(obj) < closestDist)
            {
                closestObj = obj;
            }
        }
        return closestObj;
    }

    private Vector2 FindMiddlePoint<T> (List<T> list) where T : MonoBehaviour
    {
        Vector2 middle = Vector2.zero;
        Vector2 pos;
        foreach (T obj in list)
        {
            pos = obj.transform.position;
            middle += pos / list.Count;
        }
        return middle;
    }


    private void MoveStepTo(Vector2 dir, bool reverse = false) // принимает dir вместо point тк тут это важнее
    {
        if (reverse)
        {
            dir = -dir;
        }
        gameObject.transform.Translate(dir.normalized * speed * Time.deltaTime);
    }


    private IEnumerator MoveTo(Vector2 point, bool reverse = false, float delta=0.25f)  // двигаться туда с точностью дельта
    {
        Vector2 pos = gameObject.transform.position;
        Vector2 dir;

        if (!reverse)
        {
            dir = point - pos;
            // dir.z = 0f;
        }
        else
        // идти в противоположную сторону от точки
        {
            dir = pos - point;
            point = pos + dir;
        }

        while ((pos - point).magnitude > delta)
        {
            MoveStepTo(dir);
            yield return new WaitForFixedUpdate();

            pos = gameObject.transform.position;
        }

        CheckState();
    }


    // двигается от некой точки
    private void MoveFrom(Vector2 point) // не уверен насколько правильно в плане линала
    {
        Vector2 pos = gameObject.transform.position;
        Vector2 dir = pos - point;
        StartCoroutine(MoveTo(pos + dir));
    }

    private Vector2 RandomPos(Vector2 startFrom, float range) // случайная точка вокруг заданой позиции, логично идти в место за пределами видимости
    {
        return startFrom + Random.insideUnitCircle.normalized * range;
    }
    
    private Vector2 RandomPos()
    {
        return RandomPos(gameObject.transform.position, perception * 2);
    }


    private void Death()
    {
        Destroy(gameObject);
    }


    void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug.Log("trigger существа");
    }

    void OnCollisionEnter2D(Collision2D collision) // обработка столкновения с другим существом или едой
    {
        // Debug.Log("collision существа");
        food food;
        creature creature;

        if ((food = collision.gameObject.GetComponent<food>()) != null)
        {
            if (energy < hungerLvl)
                food.EatMe(hungerLvl*3, this); // наедается с запасом
        }
        else if ((creature = collision.gameObject.GetComponent<creature>()) != null)
        {
            if (PreyInView.Contains(creature))
            {
                if (energy < hungerLvl)
                    creature.EatMe(this);
            }
        }

        CheckState();
    }

    public void EatMe(creature eater)  // пытался вынести в интервейс IEatable, но он не наследует монобехейвр
    {
        eater.energy += this.energy;
        this.Death();
    }
}
