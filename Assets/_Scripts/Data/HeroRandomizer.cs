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

        [Header("Configurações de Cores")]
        [SerializeField] private bool _randomizeColorsOnEquip = true;

        // Paleta de tons de pele realistas/fantasiosos
        private readonly Color[] _skinTones = new Color[]
        {
            new Color(1f, 0.8f, 0.6f),    // Clara
            new Color(0.9f, 0.7f, 0.5f),  // Bronzeada
            new Color(0.5f, 0.35f, 0.2f), // Escura
            new Color(0.3f, 0.2f, 0.1f),  // Negra
            new Color(0.7f, 0.85f, 0.7f)  // Orc/Esverdeada (Opcional)
        };

        // Paleta de cores para cabelo
        private readonly Color[] _hairColors = new Color[]
        {
            new Color(0.1f, 0.1f, 0.1f),  // Preto
            new Color(0.4f, 0.25f, 0.1f), // Castanho
            new Color(0.85f, 0.65f, 0.2f),// Loiro
            new Color(0.7f, 0.2f, 0.1f),  // Ruivo
            new Color(0.8f, 0.8f, 0.8f),  // Grisalho/Branco
            new Color(0.2f, 0.6f, 0.9f)   // Azul Fantasia
        };

        private HeroEditorCharacter _heroEditorCharacter;

        private void Awake()
        {
            _heroEditorCharacter = GetComponent<HeroEditorCharacter>();
        }

        public void RandomizeEquipment()
        {
            if (_heroEditorCharacter == null || _spriteCollection == null) return;

            // 1. Armadura
            if (_spriteCollection.Armor != null && _spriteCollection.Armor.Count > 0)
            {
                var randomArmor = GetRandom(_spriteCollection.Armor);
                if (randomArmor != null) _heroEditorCharacter.Armor = randomArmor.Sprites;
            }

            // 2. Capacete (70% de chance de ter capacete)
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

            // 4. Arma Principal
            if (_spriteCollection.MeleeWeapon1H != null && _spriteCollection.MeleeWeapon1H.Count > 0)
            {
                var randomWeapon = GetRandom(_spriteCollection.MeleeWeapon1H);
                if (randomWeapon != null) _heroEditorCharacter.PrimaryMeleeWeapon = randomWeapon.Sprite;
            }

            // 5. Escudo (50% de chance)
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

            // 🎯 6. Sorteia cores se a flag estiver ativa
            if (_randomizeColorsOnEquip)
            {
                RandomizeColors();
            }

            // 7. APLICA AS MUDANÇAS
            _heroEditorCharacter.Initialize();
        }

        /// <summary>
        /// 🎨 Sorteia as cores do corpo, cabelo e olhos do herói
        /// </summary>
        public void RandomizeColors()
        {
            if (_heroEditorCharacter == null) return;

            // Cor da Pele
            if (_skinTones.Length > 0)
            {
                Color skinColor = GetRandom(_skinTones);
                if (_heroEditorCharacter.BodyRenderers != null)
                {
                    foreach (var renderer in _heroEditorCharacter.BodyRenderers)
                    {
                        if (renderer != null) renderer.color = skinColor;
                    }
                }
            }

            // Cor do Cabelo
            if (_hairColors.Length > 0)
            {
                Color hairColor = GetRandom(_hairColors);
                if (_heroEditorCharacter.HairRenderer != null)
                {
                    _heroEditorCharacter.HairRenderer.color = hairColor;
                }
            }

            // Aplica atualização das cores
            _heroEditorCharacter.Initialize();
        }

        private T GetRandom<T>(IList<T> list)
        {
            if (list == null || list.Count == 0) return default;
            return list[Random.Range(0, list.Count)];
        }
    }
}