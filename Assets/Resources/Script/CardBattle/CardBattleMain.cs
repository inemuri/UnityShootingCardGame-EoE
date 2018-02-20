using System;
using System.Collections.Generic;
using System.Linq;

/*
 * 卡牌战斗系统主处理
 */
public class CardBattleMain
{
    public CardBattleData data = new CardBattleData();              // 当前对局数据
    public CardBattleData dataStgStart = new CardBattleData();      // STG阶段开始时的对局数据(用于记录变化量)
    public List<CardSkill> activeSkills = new List<CardSkill>();    // 生效中技能
    public List<Card> playerDeck = new List<Card>();                // 完整卡组
    public List<Card> playerLib = new List<Card>();                 // 牌库
    public List<Card> playerHand = new List<Card>();                // 手牌
    public List<Card> playerGrave = new List<Card>();               // 墓地
    public List<Card> playerStack = new List<Card>();               // 结算堆叠

    public CardBattleMain(List<Card> startDeck)
    {
        // 设置初始卡组
        playerDeck.AddRange(startDeck);
        playerLib.AddRange(playerDeck.Where(c => c.nowIsIn == CardPosition.None));
        playerLib.ForEach(c => c.nowIsIn = CardPosition.Library);
        playerLib.Shuffle();
    }

    // 执行指定阶段处理
    public void Run(BattlePhase phase)
    {
        switch (phase)
        {
            case BattlePhase.TurnStart:
                TurnStartProcess();
                break;
            case BattlePhase.Draw:
                DrawProcess();
                break;
            case BattlePhase.Play:
                PlayProcess();
                break;
            case BattlePhase.TurnEnd:
                TurnEndProcess();
                break;
            default:
                return;
        }
    }

    // 检查回合开始时生效效果
    private void TurnStartProcess()
    {
        activeSkills.ForEach(s =>
        {
            s.TurnCheck();
            s.SkillEffect(this);
            if (s.turnDuration == 0)
            {
                activeSkills.Remove(s);
            }
        });
    }

    // 抽牌
    private void DrawProcess()
    {
        while (playerHand.Count < data.HandLimit)
        {
            // 牌库空后将墓地洗入牌库
            if (playerLib.Count == 0)
            {
                // 墓地也无牌可用时取消抽牌
                if (playerGrave.Count == 0)
                {
                    break;
                }
                playerGrave.ForEach(c => c.nowIsIn = CardPosition.Library);
                playerDeck.AddRange(playerGrave);
                playerGrave.Clear();
                playerLib.Shuffle();
            }
            // 抽牌库顶的一张牌放入手牌
            Card draw = playerLib[0];
            playerHand.Add(draw);
            playerLib.Remove(draw);
            draw.nowIsIn = CardPosition.Hand;
        }
    }

    // 结算
    private void PlayProcess()
    {
        // 将选中的手牌加入堆叠
        playerHand.ForEach(c =>
        {
            if (c.selected)
            {
                playerStack.Add(c);
                c.nowIsIn = CardPosition.Graveyard;
            }
        });
        // 将牌组中符合生效区域的卡牌加入堆叠
        playerStack.AddRange(playerDeck.Where(c => CardSkill.checkAera(c.Id, c.nowIsIn)).ToList());
        // 开始结算
        playerStack.ForEach(c => CardSkill.CardEffectProcess(c.Id, this));
    }

    // 回合结束
    private void TurnEndProcess()
    {
        // 将结算完的手牌丢入墓地
        playerHand.RemoveAll(c =>
        {
            if (c.selected)
            {
                c.selected = false;
                playerGrave.Add(c);
                return true;
            }
            else
            {
                return false;
            }
        });
        playerStack.Clear();
        data.CopyTo(dataStgStart);
    }

}


/*
 * 对局数据容器
 */
public class CardBattleData
{
    public int turns = 1;
    public int life = 0;
    private int handLimit = 1;

    public int HandLimit { set { handLimit = value < 0 ? 0 : value; } get { return handLimit; } }

    public void CopyTo(CardBattleData target)
    {
        target.turns = turns;
        target.handLimit = handLimit;
        target.life = life;
    }
}


/*
 * 卡牌技能和结算相关处理
 */
public class CardSkill
{

    public int skillId;                                               // 技能ID
    public int turnDuration;                                          // 生效的回合数
    private CardBattleData dataWhilePlay = new CardBattleData();      // 记录技能设置时的对局数据
    private Predicate<CardBattleMain> trigger;                        // 触发条件，为空时默认触发，在初始化时设置

    // 手牌中生效的卡牌id
    public static int[] handWork = { 1, 2, 3 };
    // 牌库中生效的卡牌id
    public static int[] libWork = { };
    // 墓地中生效的卡牌id
    public static int[] graveWork = { };
    // 只要在牌组中就生效的卡牌id
    public static int[] deckWork = { 4 };
    // 检测生效区域
    public static Func<int, CardPosition, bool> checkAera = ((cid, pos) =>
    {
        if (deckWork.Contains(cid)) return true;
        switch (pos)
        {
            case CardPosition.Library:
                return libWork.Contains(cid);
            case CardPosition.Hand:
                return handWork.Contains(cid);
            case CardPosition.Graveyard:
                return graveWork.Contains(cid);
        }
        return false;
    });

    CardSkill(int id, CardBattleData d)
    {
        d.CopyTo(dataWhilePlay);
        skillId = id;
        SetSkill(id);
    }

    // 初始化属性
    private void SetSkill(int id)
    {
        switch (id)
        {
            case 999:
                // 设置触发条件
                trigger = ((battle) =>
                {
                    return (turnDuration == 0 && dataWhilePlay.life - battle.data.life > 5);
                });
                turnDuration = 1;
                break;
            default:
                turnDuration = 1;
                break;
        }
    }


    // 触发
    public void SkillEffect(CardBattleMain battle)
    {
        // 触发条件检查
        if (trigger == null || trigger.Invoke(battle))
        {
            switch (skillId)
            {
                default:
                    break;
            }
        }
    }

    // 卡牌效果结算
    public static void CardEffectProcess(int id, CardBattleMain battle)
    {
        switch (id)
        {
            default:
                break;
        }
    }

    // 回合开始时检查
    public bool TurnCheck()
    {
        turnDuration--;
        return turnDuration <= 0;
    }

}



/*
 * List扩展洗牌方法
 */
public static class ListEx
{
    public static void Shuffle<T>(this List<T> list)
    {
        list = list.OrderBy(l => Guid.NewGuid()).ToList();
    }
}



// 当前对局阶段
public enum BattlePhase
{
    Prepare = -1,
    TurnStart = 0,
    Draw = 1,
    Play = 2,
    TurnEnd = 3,
}

// 卡牌位置
public enum CardPosition
{
    None, Library, Hand, Graveyard, Exile
}