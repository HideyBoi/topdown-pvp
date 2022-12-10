using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    float timeToStartFade = 3;
    float timeToDelete = 1;
    int currentDamage = 0;
    Color currentColor = Color.white;

    [SerializeField] TextMeshPro numberText;
    [SerializeField] Animator animator;

    [SerializeField] float maxRandAngle = 23;
    [SerializeField] float colorChangeSpeed = 0.1f;
    [SerializeField] Color defaultColor;
    [SerializeField] int yellowPercent;
    [SerializeField] Color yellowColor;
    [SerializeField] int redPercent;
    [SerializeField] Color redColor;

    public void AddNumber(int toAdd, Vector3 pos)
    {
        currentDamage += toAdd;
        timeToStartFade = 3;
        timeToDelete = 1;
        transform.position = pos;

        animator.Play("Jump");

        transform.rotation = Quaternion.Euler(0, Random.Range(-maxRandAngle, maxRandAngle), 0);
    }

    private void FixedUpdate()
    {
        numberText.text = currentDamage.ToString();

        if (timeToStartFade > 0)
        {
            timeToStartFade -= Time.fixedDeltaTime;
        } else
        {
            timeToDelete -= Time.fixedDeltaTime;

            if (timeToDelete < 0)
            {
                Destroy(gameObject);
            }
        }

        Color targetColor = defaultColor;

        if (currentDamage >= (RulesManager.instance.maxHealth * (redPercent * 0.01f)))
        {
            targetColor = redColor;
        } else if (currentDamage >= (RulesManager.instance.maxHealth * (yellowPercent * 0.05f)))
        {
            targetColor = yellowColor;
        }

        numberText.color = Color.Lerp(numberText.color, targetColor, colorChangeSpeed);
        numberText.color = new Color(numberText.color.r, numberText.color.g, numberText.color.b, timeToDelete);
    }
}
