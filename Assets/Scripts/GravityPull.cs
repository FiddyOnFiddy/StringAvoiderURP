using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GravityPull : MonoBehaviour
{
    [FormerlySerializedAs("keyRB")] [SerializeField] Rigidbody2D keyRb;
    [SerializeField] public bool hasKey;
    [SerializeField] float distance;
    [SerializeField] Vector2 direction;
    [SerializeField] float activationDistance;
    [SerializeField] float gravityRange;
    [SerializeField] float minForce, maxForce;
    [SerializeField] float minDrag, maxDrag;
    [SerializeField] float force, drag;
    [SerializeField] float distancePercentage;
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Color defaultColor;

    // Start is called before the first frame update
    void Start()
    {
        minDrag = keyRb.drag;
        sprite = GetComponent<SpriteRenderer>();
        defaultColor = sprite.color;
    }

    // Update is called once per frame
    void Update()
    {
        direction = transform.position - keyRb.transform.position;
        distance = direction.magnitude;
        if (distance < activationDistance)
        {
            hasKey = true;
        }
        else
        {
            hasKey = false;
        }

        GravitationalPull();

        ChangeAlpha();

    }

    void GravitationalPull()
    {
        if (distance < gravityRange && distance > 0)
        {
            CalculatePercentage();

            force = Mathf.Lerp(maxForce, minForce, distancePercentage);
            drag = Mathf.Lerp(maxDrag, minDrag, distancePercentage);


            keyRb.AddForce(force * direction);
            keyRb.drag = drag;
        }
    }

    void CalculatePercentage()
    {
        distancePercentage = (distance * (100 / gravityRange)) / 100f;
    }

    void ChangeAlpha()
    {
        if (hasKey == true)
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, Mathf.MoveTowards(sprite.color.a, 1, 0.1f));
        }
        else
        {
            sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, Mathf.MoveTowards(sprite.color.a, defaultColor.a, 0.1f));
        }
    }
}
