using System.Collections.Generic;
using UnityEngine;
using HeroEditor.Common;
using HeroEditorCharacter = Assets.HeroEditor.Common.Scripts.CharacterScripts.Character;

namespace DungeonKeeper
{
    public class HeroRandomizer : MonoBehaviour
    {
        [Header("Referência da Coleção de Sprites")]
        [SerializeField] private SpriteCollection _spriteCollection;

        private HeroEditorCharacter _heroEditorCharacter;

        private void Awake()
        {
            _heroEditorCharacter = GetComponent<HeroEditorCharacter>();
        }

        /// <summary>
        /// Sorteia peças de equipamentos aleatórias direto da SpriteCollection
        /// </summary>
        public void RandomizeEquipment()
        {
            if (_heroEditorCharacter == null || _spriteCollection == null) return;

            // 1. Armadura
            if (_spriteCollection.Armor != null && _spriteCollection.Armor.Count > 0)
            {
                var randomArmor = GetRandom(_spriteCollection.Armor);
                if (randomArmor != null) _heroEditorCharacter.Armor = randomArmor.Sprites;
            }

            // 2. Capacete (70% de chance de ter capacete, 30% sem)
            if (_spriteCollection.Helmet != null && _spriteCollection.Helmet.Count > 0 && Random.value > 0.3f)
            {
                _heroEditorCharacter.ShowHelmet = true;
                var randomHelmet = GetRandom(_spriteCollection.Helmet);
                if (randomHelmet != null) _heroEditorCharacter.Helmet = randomHelmet.Sprite;
            }
            else
            {
                _heroEditorCharacter.ShowHelmet = false;
                _heroEditorCharacter.Helmet = null;
            }

            // 3. Cabelo
            if (_spriteCollection.Hair != null && _spriteCollection.Hair.Count > 0)
            {
                var randomHair = GetRandom(_spriteCollection.Hair);
                if (randomHair != null) _heroEditorCharacter.Hair = randomHair.Sprite;
            }

            // 4. Arma Principal (Usando a lista MeleeWeapon1H com 111 variações!)
            if (_spriteCollection.MeleeWeapon1H != null && _spriteCollection.MeleeWeapon1H.Count > 0)
            {
                var randomWeapon = GetRandom(_spriteCollection.MeleeWeapon1H);
                if (randomWeapon != null) _heroEditorCharacter.PrimaryMeleeWeapon = randomWeapon.Sprite;
            }

            // 5. Escudo (50% de chance de ter escudo)
            if (_spriteCollection.Shield != null && _spriteCollection.Shield.Count > 0 && Random.value > 0.5f)
            {
                _heroEditorCharacter.WeaponType = HeroEditor.Common.Enums.WeaponType.Melee1H;
                var randomShield = GetRandom(_spriteCollection.Shield);
                if (randomShield != null) _heroEditorCharacter.Shield = randomShield.Sprite;
            }
            else
            {
                _heroEditorCharacter.Shield = null;
            }

            // 6. APLICA AS MUDANÇAS NO VISUAL!
            _heroEditorCharacter.Initialize();
        }

        private T GetRandom<T>(List<T> list)
        {
            if (list == null || list.Count == 0) return default;
            return list[Random.Range(0, list.Count)];
        }
    }
}