﻿using Item.Ammo;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace GameSystem.Skill
{
    [CreateAssetMenu(menuName = "GameSystem/Skill/Shell Rain Skill")]
    public class ShellRainSkill : SkillObject
    {
        public ObjectPool shellPool;            //炮弹池
        public ObjectPool warnningPool;         //警告区域特效池
        [Range(0, 10f)]
        public float skillDelay = 1.5f;         //技能释放延迟
        [Range(1, 100)]
        public int skillLevel = 6;              //技能等级
        [Range(1, 100f)]
        public float attackRadius = 5f;         //技能攻击范围半径
        [Range(0, 1f)]
        public float attackRate = 0.3f;         //技能每次释放频率
        [Range(0, 100f)]
        public float attackDamage = 30f;        //每一粒炮弹最大伤害

        /// <summary>
        /// 初始化警告区域位置
        /// </summary>
        public override void Init()
        {
            warnningPool.CreateObjectPool(GameObject.FindGameObjectWithTag("GroundCanvas"));
            aimMode.groundSpriteRadius = attackRadius;
        }

        /// <summary>
        /// 技能效果
        /// </summary>
        public override IEnumerator SkillEffect(PlayerManager launcher)
        {
            Vector3 position = AllSkillManager.Instance.aim.HitPosition;
            ShowWarnningArea(position);             // 显示警告区域，在一段时间后再发起攻击
            yield return new WaitForSeconds(skillDelay);
            for (int i = 0; i < skillLevel; i++)                // 根据技能等级改变攻击波数
                yield return CreateShell(position, Random.insideUnitCircle * attackRadius, launcher);
        }

        /// <summary>
        /// 每隔一段时间创建炮弹
        /// </summary>
        /// <param name="randomCircle">创建的XZ坐标</param>
        /// <returns></returns>
        private IEnumerator CreateShell(Vector3 inputPosition, Vector2 randomCircle, PlayerManager launcher)
        {
            //if (gamePlaying == false)               //不在游戏进行中就终结他，暂未解决使用StopCoroutine无效的问题 'Skill' 175行
            //    yield break;            
            //创建炮弹 从上而下
            ShellAmmo shell = shellPool.GetNextObject().GetComponent<ShellAmmo>();
            shell.Init(launcher, attackDamage);
            shell.transform.position = new Vector3(inputPosition.x + randomCircle.x, 20f, inputPosition.z + randomCircle.y);
            shell.transform.rotation = Quaternion.Euler(new Vector3(90f, 0, 0));
            shell.GetComponent<Rigidbody>().velocity = new Vector3(0, -20f, 0);
            shell.GetComponent<ShellAmmo>().damage = attackDamage;
            yield return new WaitForSeconds(Random.Range(0, attackRate * 2));
        }

        /// <summary>
        /// 显示警告区域，设置位置大小持续时间等
        /// </summary>
        /// <param name="position">显示的位置</param>
        private void ShowWarnningArea(Vector3 position)
        {
            GameObject warnningArea = warnningPool.GetNextObject(false);
            //设置警告区域显示闪烁时间、持续时间
            WarnningArea areaScript = warnningArea.GetComponent<WarnningArea>();
            areaScript.blinkDuration = skillDelay;
            areaScript.duration = skillDelay + attackRate * skillLevel + areaScript.endDuration;
            //设置警告区域位置大小
            RectTransform imageTransform = warnningArea.GetComponent<Image>().rectTransform;
            imageTransform.position = new Vector3(position.x, position.y + 0.5f, position.z);
            imageTransform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
            imageTransform.sizeDelta = new Vector2(attackRadius * 2, attackRadius * 2);

            //最后再激活，因为有些初始化配置在OnEnable()
            warnningArea.SetActive(true);
        }

        /// <summary>
        /// 使用默认释放判断条件，直接返回true
        /// </summary>
        /// <returns></returns>
        public override bool ReleaseCondition()
        {
            return true;
        }
    }
}