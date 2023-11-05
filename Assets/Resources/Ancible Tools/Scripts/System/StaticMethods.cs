using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Assets.Resources.Ancible_Tools.Scripts.Hitbox;
using Assets.Resources.Ancible_Tools.Scripts.System.Items;
using Assets.Resources.Ancible_Tools.Scripts.System.WorldInput;
using Assets.Resources.Ancible_Tools.Scripts.Traits;
using CauldronOnlineCommon;
using CauldronOnlineCommon.Data;
using CauldronOnlineCommon.Data.Combat;
using CauldronOnlineCommon.Data.Math;
using CauldronOnlineCommon.Data.ObjectParameters;
using CauldronOnlineCommon.Data.Zones;
using ConcurrentMessageBus;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    public static class StaticMethods
    {
        public const float CORNER_DIRECTION_DIFFERENCE = .33f;

        private static Regex STRIP_HTML = new Regex("<.*?>", RegexOptions.Compiled);

        public static List<T> QueryList<T>(this List<T> list, Predicate<T> query)
        {
            var returnList = new List<T>();
            for (var i = 0; i < list.Count; i++)
            {
                if (query.Invoke(list[i]))
                {
                    returnList.Add(list[i]);
                }
            }
            return returnList;
        }

        public static NullValue<T> QuerySingle<T>(this List<T> list, Predicate<T> query)
        {
            var returnValue = new NullValue<T>();
            for (var i = 0; i < list.Count; i++)
            {
                if (query.Invoke(list[i]))
                {
                    returnValue.SetValue(list[i]);
                    break;
                }
            }
            return returnValue;

        }

        public static void LerpMove(this Rigidbody2D rigidbody, float moveSpeed, float interpolation, Vector2 direction)
        {
            var position = rigidbody.position;
            position += Vector2.ClampMagnitude(moveSpeed * direction, moveSpeed);
            position = Vector2.Lerp(rigidbody.position, position, interpolation);
            //position.x = (float)Math.Round(position.x, DataController.ROUND_DECIMAL_PLACES);
            //position.y = (float)Math.Round(position.y, DataController.ROUND_DECIMAL_PLACES);
            rigidbody.position = position;
        }

        public static IEnumerator WaitForFrames(int frames, Action doAfter)
        {
            var frameCount = Time.frameCount + frames;
            yield return new WaitUntil(() =>
            {
                var currentFrameCount = Time.frameCount;
                return currentFrameCount >= frameCount;
            });
            doAfter.Invoke();
        }

        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        public static HitboxController SetupHitbox(this GameObject gameObject, Hitbox.Hitbox hitbox, CollisionLayer layer)
        {
            var hitboxFilter = HitboxController.GenerateHitboxFilter(hitbox, layer);
            var hitboxCheckmsg = MessageFactory.GenerateHitboxCheckMsg();
            HitboxController controller = null;
            hitboxCheckmsg.DoAfter = hitboxController =>
            {
                controller = hitboxController;
            };
            var parentObj = gameObject;
            if (gameObject.transform.parent)
            {
                parentObj = gameObject.transform.parent.gameObject;
            }
            gameObject.SendMessageWithFilterTo(hitboxCheckmsg, parentObj, hitboxFilter);
            MessageFactory.CacheMessage(hitboxCheckmsg);
            if (!controller)
            {
                controller = Object.Instantiate(hitbox.Controller, parentObj.transform);
                controller.Setup(hitbox, layer);
            }
            return controller;
        }

        //public static Vector2 ToPixelPosition(this Vector2 vector)
        //{
        //    var returnValue = vector;

        //    var xMultiplier = vector.x / DataController.Interpolation;
        //    var trueXMulti = xMultiplier;
        //    var intX = 0;
        //    if (xMultiplier < 0f)
        //    {
        //        trueXMulti *= -1;

        //    }
        //    intX = Mathf.RoundToInt(trueXMulti);

        //    if (xMultiplier < 0f)
        //    {
        //        intX *= -1;
        //    }

        //    returnValue.x = intX * DataController.Interpolation;

        //    var yMultiplier = vector.y / DataController.Interpolation;
        //    var trueYMulti = yMultiplier;
        //    var intY = 0;
        //    if (yMultiplier < 0f)
        //    {
        //        trueYMulti *= -1;

        //    }
        //    intY = Mathf.RoundToInt(trueYMulti);

        //    if (yMultiplier < 0f)
        //    {
        //        intY *= -1;
        //    }

        //    returnValue.y = intY * DataController.Interpolation;

        //    return returnValue;
        //}

        public static Vector2 ToStaticDirections(this Vector2 vector)
        {
            var returnVector = vector;
            if (returnVector.x > 0)
            {
                returnVector.x = 1;
            }
            else if (returnVector.x < 0)
            {
                returnVector.x = -1;
            }

            if (returnVector.y > 0)
            {
                returnVector.y = 1;
            }
            else if (returnVector.y < 0)
            {
                returnVector.y = -1;
            }

            return returnVector;
        }

        public static Vector2Int ToVector2Int(this Vector2 vector, bool rounding = false)
        {
            if (rounding)
            {
                return new Vector2Int(Mathf.RoundToInt(vector.x), Mathf.RoundToInt(vector.y));
            }
            return new Vector2Int((int)vector.x, (int)vector.y);

        }

        public static int GetHeightOfText(this Text uiObject, string text)
        {
            var generator = new TextGenerator();
            var settings = uiObject.GetGenerationSettings(uiObject.rectTransform.rect.size);
            settings.verticalOverflow = VerticalWrapMode.Overflow;
            settings.horizontalOverflow = HorizontalWrapMode.Wrap;
            //settings.fontSize = uiObject.fontSize;
            //settings.font = uiObject.font;
            settings.lineSpacing = uiObject.lineSpacing;
            generator.Populate(text, settings);
            //Debug.Log($"Line Count: {generator.lines.Count}");
            var height = generator.GetPreferredHeight(text, settings);
            var intHeight = (int) height;
            if (intHeight < height)
            {
                intHeight += 1;
            }

            var halfHeight = intHeight / 2f;
            var halfIntHeight = intHeight / 2;
            if (halfHeight > halfIntHeight)
            {
                intHeight += 1;
            }
            return intHeight;
            //return generator.GetPreferredHeight(text, settings) /*/ (settings.font.fontSize / (float)settings.fontSize)*/;
        }

        public static float GetWidthText(this Text uiObject, string text)
        {
            var generator = new TextGenerator();
            var settings = uiObject.GetGenerationSettings(uiObject.rectTransform.rect.size);
            settings.horizontalOverflow = uiObject.horizontalOverflow;
            settings.fontSize = uiObject.fontSize;
            return generator.GetPreferredWidth(text, settings) / (settings.font.fontSize / (float)settings.fontSize);
        }

        public static string[] GetFormmatedTextLines(this Text uiObject, string text)
        {
            var generator = new TextGenerator();
            var settings = uiObject.GetGenerationSettings(uiObject.rectTransform.rect.size);
            settings.verticalOverflow = VerticalWrapMode.Overflow;
            generator.Populate(text, settings);
            var lines = generator.lines;
            var formmatedLines = new List<string>();
            for (var i = 0; i < lines.Count; i++)
            {
                var startPosition = lines[i].startCharIdx;
                var endPosition = text.Length;
                if (i < lines.Count - 1)
                {
                    endPosition = lines[i + 1].startCharIdx;
                }
                var line = text.Substring(startPosition, endPosition - startPosition);
                line = line.TrimEnd(' ');
                line = line.Replace(Environment.NewLine, string.Empty);
                formmatedLines.Add(line);
            }
            //formmatedLines.RemoveAll(string.IsNullOrEmpty);

            return formmatedLines.ToArray();
        }

        public static string ToSingleString(this string[] text)
        {
            var returnValue = string.Empty;
            for (var i = 0; i < text.Length; i++)
            {
                returnValue = i == 0 ? text[i] : $"{returnValue}{Environment.NewLine}{text[i]}";
            }
            return returnValue;
        }

        public static Vector2 GetMouseQuadrant(Vector2 mousePos)
        {
            var middleScreen = new Vector2(Screen.width / 2f, Screen.height / 2f);
            var quadrant = Vector2.zero;
            if (mousePos.x > middleScreen.x)
            {
                quadrant.x = 1;
            }

            if (mousePos.y > middleScreen.y)
            {
                quadrant.y = 1;
            }

            return quadrant;
        }

        public static Vector2 ToNakedValues(this Vector2 vector)
        {
            var returnVector = vector;
            if (returnVector.x < 0)
            {
                returnVector.x *= -1;
            }

            if (returnVector.y < 0)
            {
                returnVector.y *= -1;
            }

            return returnVector;
        }

        public static Vector2Int ToCardinal(this Vector2 vector)
        {
            var naked = vector.ToNakedValues();
            var returnValue = Vector2Int.zero;
            if (naked.x > naked.y)
            {
                returnValue.x = vector.x > 0 ? 1 : -1;
            }
            else if (naked.y > naked.x)
            {
                returnValue.y = vector.y > 0 ? 1 : -1;
            }

            return returnValue;
        }

        public static Vector2Int ToDirection(this Vector2 vector)
        {
            var direction = Vector2Int.zero;
            if (vector.y > 0)
            {
                direction.y = 1;
            }
            else if (vector.y < 0)
            {
                direction.y = -1;
            }

            if (vector.x > 0)
            {
                direction.x = 1;
            }
            else if (vector.x < 0)
            {
                direction.x = -1;
            }

            return direction;
        }

        public static int Roll(this IntNumberRange range)
        {
            return Random.Range(range.Minimum, range.Maximum + 1);
        }

        public static float Roll(this FloatNumberRange range)
        {
            return Random.Range(range.Minimum, range.Maximum);
        }

        public static bool CoinFlip()
        {
            return Random.Range(0f, 1f) >= .5f;
        }

        public static string ApplyColorToText(string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }

        public static float ToZRotation(this Vector2 direction)
        {
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        public static void SetTransformPosition(this Transform transform, Vector2 position)
        {
            var pos = transform.position;
            pos.x = position.x;
            pos.y = position.y;
            transform.position = pos;
        }

        public static void SetLocalPosition(this Transform transform, Vector2 position)
        {
            var pos = transform.localPosition;
            pos.x = position.x;
            pos.y = position.y;
            transform.localPosition = pos;
        }

        public static string[] GetTraitDescriptions(this Trait[] traits)
        {
            return traits.Select(t => t.GetDescription()).Where(t => !string.IsNullOrEmpty(t)).ToArray();
        }

        public static Vector3 ToVector3(this Vector2 vector)
        {
            return new Vector3(vector.x, vector.y);
        }

        public static Vector3 ToVector3(this Vector2 vector, float z)
        {
            return new Vector3(vector.x, vector.y, z);
        }

        public static void SetLocalScaling(this Transform transform, Vector2 scale)
        {
            var localScale = transform.localScale;
            localScale.x = scale.x;
            localScale.y = scale.y;
            transform.localScale = localScale;
        }

        public static void SetLocalRotation(this Transform transform, float rotation)
        {
            var euler = transform.localRotation.eulerAngles;
            euler.z = rotation;
            transform.localRotation = Quaternion.Euler(euler);
        }

        public static string ReplaceSpacesWithUnderscores(this string text)
        {
            return text.Replace(" ", "_");
        }

        public static Vector2 ToPixelPerfect(this Vector2 vector)
        {
            var intVector = ((vector /*+ DataController.TrueZero*/) / DataController.Interpolation).ToVector2Int(true);
            var returnVector = new Vector2(intVector.x * DataController.Interpolation, intVector.y * DataController.Interpolation);
            //var diff = vector - returnVector;
            //if (diff.x < -DataController.TrueZero.x / 2f)
            //{
            //    returnVector.x -= DataController.Interpolation;
            //}
            //else if (diff.x > DataController.TrueZero.x / 2f)
            //{
            //    returnVector.x += DataController.Interpolation;
            //}

            //if (diff.y < -DataController.TrueZero.y / 2f)
            //{
            //    returnVector.y -= DataController.Interpolation;
            //}
            //else if (diff.y > DataController.TrueZero.y / 2f)
            //{
            //    returnVector.y += DataController.Interpolation;
            //}

            return returnVector;
        }

        public static WorldVector2Int ToWorldVector(this Vector2Int vector)
        {
            return new WorldVector2Int(vector.x, vector.y);
        }

        public static Vector2Int ToVector(this WorldVector2Int vector)
        {
            return new Vector2Int(vector.X, vector.Y);
        }

        public static Vector2 ToVector2(this Vector2Int vector, bool interpolated)
        {
            return interpolated ? new Vector2(vector.x * (DataController.Interpolation + Vector2.kEpsilon), vector.y * (DataController.Interpolation + Vector2.kEpsilon)) : new Vector2(vector.x, vector.y);
        }

        public static Vector2 ToWorldVector(this WorldVector2Int vector)
        {
            return new Vector2(vector.X * (DataController.Interpolation + Vector2.kEpsilon), vector.Y * (DataController.Interpolation + Vector2.kEpsilon));
        }

        public static Vector2 Multiply(this Vector2Int direction, float multiply = 1f)
        {
            return new Vector2(direction.x * multiply, direction.y * multiply);
        }

        public static Vector2Int NakedValues(this Vector2Int vector)
        {
            var returnValue = vector;
            if (vector.x < 0)
            {
                returnValue.x *= -1;
            }

            if (vector.y < 0)
            {
                returnValue.y *= -1;
            }

            return returnValue;
        }

        public static Vector2 Absolute(this Vector2 vector)
        {
            var returnValue = vector;
            if (vector.x < 0)
            {
                returnValue.x *= -1;
            }

            if (vector.y < 0)
            {
                returnValue.y *= -1;
            }

            return returnValue;
        }

        public static WorldVector2Int ToWorldPosition(this Vector2 vector)
        {
            var pos = vector / (DataController.Interpolation);
            return new WorldVector2Int((int)pos.x, (int)pos.y);
        }

        public static Vector2Int ToKnockbackDirection(this Vector2 vector)
        {
            var direction = Vector2Int.zero;
            var nakedValues = vector.ToNakedValues();
            var min = Mathf.Min(nakedValues.x, nakedValues.y);
            var max = Mathf.Max(nakedValues.x, nakedValues.y);
            if (max - min <= CORNER_DIRECTION_DIFFERENCE)
            {
                direction.x = vector.x > 0 ? 1 : -1;
                direction.y = vector.y > 0 ? 1 : -1;
            }
            else if (nakedValues != Vector2.zero)
            {
                if (nakedValues.x > nakedValues.y)
                {
                    direction.x = vector.x > 0 ? 1 : -1;
                }
                else
                {
                    direction.y = vector.y > 0 ? 1 : -1;
                }
            }
            return direction;
        }

        public static Vector2Int ToFaceDirection(this Vector2Int vector)
        {
            var direction = Vector2Int.zero;
            var nakedValues = vector.NakedValues();
            if (nakedValues.x > nakedValues.y)
            {
                direction.x = vector.x > 0 ? 1 : -1;
                direction.y = 0;
            }
            else if (nakedValues.y > nakedValues.x)
            {
                direction.y = vector.y > 0 ? 1 : -1;
                direction.x = 0;
            }

            return direction;
        }

        public static Vector2Int ToFaceDirection(this Vector2 vector)
        {
            var direction = Vector2Int.zero;
            var nakedValues = vector.Absolute();
            if (nakedValues.x > nakedValues.y)
            {
                direction.x = vector.x > 0 ? 1 : -1;
                direction.y = 0;
            }
            else if (nakedValues.y > nakedValues.x)
            {
                direction.y = vector.y > 0 ? 1 : -1;
                direction.x = 0;
            }

            return direction;
        }

        public static int Roll(this WorldIntRange range, bool inclusive)
        {
            return Random.Range(range.Min, inclusive ? range.Max + 1 : range.Max );
        }

        public static Vector2 RandomDirection()
        {
            return new Vector2(Random.Range(-1f,1f), Random.Range(-1f,1f));
        }

        public static string ToDescriptionString(this CombatStats stats)
        {
            var descriptionStrings = new List<string>();
            if (stats.Health != 0)
            {
                descriptionStrings.Add(ApplyColorToText($"{stats.Health.ToStatString(CombatStats.MAX_HEALTH)}", stats.Health > 0 ? ColorFactory.PositiveStat : ColorFactory.NegativeStat));
                
            }

            if (stats.Mana != 0)
            {
                descriptionStrings.Add(ApplyColorToText($"{stats.Mana.ToStatString(CombatStats.MAX_MANA)}", stats.Mana > 0 ? ColorFactory.PositiveStat : ColorFactory.NegativeStat));
            }

            if (stats.Armor != 0)
            {
                descriptionStrings.Add(ApplyColorToText($"{stats.Armor.ToStatString(CombatStats.ARMOR)}", stats.Armor > 0 ? ColorFactory.PositiveStat : ColorFactory.NegativeStat));
            }

            if (stats.Strength != 0)
            {
                descriptionStrings.Add(ApplyColorToText($"{stats.Strength.ToStatString(CombatStats.STRENGTH)}", stats.Strength > 0 ? ColorFactory.PositiveStat : ColorFactory.NegativeStat));
            }

            if (stats.Agility != 0)
            {
                
                descriptionStrings.Add(ApplyColorToText($"{stats.Agility.ToStatString(CombatStats.AGILITY)}", stats.Agility > 0 ? ColorFactory.PositiveStat : ColorFactory.NegativeStat));
            }

            if (stats.Wisdom != 0)
            {
                
                descriptionStrings.Add(ApplyColorToText($"{stats.Wisdom.ToStatString(CombatStats.WISDOM)}", stats.Wisdom > 0 ? ColorFactory.PositiveStat : ColorFactory.NegativeStat));
            }

            if (stats.Luck != 0)
            {                
                descriptionStrings.Add(ApplyColorToText($"{stats.Luck.ToStatString(CombatStats.LUCK)}", stats.Luck > 0 ? ColorFactory.PositiveStat : ColorFactory.NegativeStat));
            }

            var returnString = string.Empty;
            foreach (var description in descriptionStrings)
            {
                returnString = string.IsNullOrEmpty(returnString) ? $"{description}" : $"{returnString}{Environment.NewLine}{description}";
            }

            return returnString;
        }

        public static string RemoveTags(this string text)
        {
            return STRIP_HTML.Replace(text, string.Empty);
        }

        public static void AddParameter(this ObjectSpawnData data, ObjectParameter parameter)
        {
            var parameters = data.Parameters.ToList();
            parameters.Add(parameter);
            data.Parameters = parameters.ToArray();
        }

        public static void AddTrait(this ObjectSpawnData data, string trait)
        {
            var traits = data.Traits.ToList();
            if (!traits.Contains(trait))
            {
                traits.Add(trait);
            }

            data.Traits = traits.ToArray();
        }

        public static string[] ToDisplayNames(this BonusTag[] tags)
        {
            var returnList = new List<string>();
            foreach (var tag in tags)
            {
                returnList.Add(tag.DisplayName);
            }

            return returnList.ToArray();
        }

        public static string ToSingleLine(this BonusTag[] tags)
        {
            var returnString = string.Empty;
            var names = tags.ToDisplayNames();
            if (names.Length > 1)
            {
                for (var i = 0; i < names.Length; i++)
                {
                    if (string.IsNullOrEmpty(returnString))
                    {
                        if (i == 0)
                        {
                            returnString = $"{names[i]},";
                        }
                        else if (i < names.Length - 1)
                        {
                            returnString = $"{returnString}{names[i]},";
                        }
                        else
                        {
                            returnString = $"{returnString}{names[i]}";
                        }

                    }
                }
            }
            else if (names.Length > 0)
            {
                returnString = $"{names[0]}";
            }

            return returnString;
        }

        public static string[] GetRequiredStacks(this ResourceItemStack[] stacks)
        {
            var returnList = new List<string>();
            foreach (var stack in stacks)
            {
                returnList.Add(stack.GetRequiredDescription());
            }

            return returnList.ToArray();
        }

        public static string ToCommaDelimitedLine(this string[] items)
        {
            var returnString = string.Empty;
            for (var i = 0; i < items.Length; i++)
            {
                if (i == 0)
                {
                    returnString = items.Length <= 1 ? $"{items[i]}" : $"{returnString} {items[i]},";
                }
                else if (i < items.Length - 1)
                {
                    returnString = $"{returnString} {items[i]},";
                }
                else
                {
                    returnString = $"{returnString} {items[i]}";
                }
            }

            return returnString;
        }

        public static string ToPlayTime(this TimeSpanData data)
        {
            return $"{data.Days:N0}:{data.Hours}:{data.Minutes}";
        }

        public static string ToInputString(this Key key)
        {
            switch (key)
            {
                case Key.UpArrow:
                    return WorldKeyboardInputLayout.UP;
                case Key.DownArrow:
                    return WorldKeyboardInputLayout.DOWN;
                case Key.LeftArrow:
                    return WorldKeyboardInputLayout.LEFT;
                case Key.RightArrow:
                    return WorldKeyboardInputLayout.RIGHT;
                case Key.LeftShift:
                    return WorldKeyboardInputLayout.LEFT_SHIFT;
                case Key.RightShift:
                    return WorldKeyboardInputLayout.RIGHT_SHIFT;
                default:
                        return $"{key}";
            }
        }

        public static bool ToInputState(this WorldGamepadInputType type, Gamepad gamepad)
        {
            switch (type)
            {
                case WorldGamepadInputType.LStickUp:
                    return gamepad.leftStick.up.isPressed;
                case WorldGamepadInputType.LStickDown:
                    return gamepad.leftStick.down.isPressed;
                case WorldGamepadInputType.LStickLeft:
                    return gamepad.leftStick.left.isPressed;
                case WorldGamepadInputType.LStickRight:
                    return gamepad.leftStick.right.isPressed;
                case WorldGamepadInputType.RStickUp:
                    return gamepad.rightStick.up.isPressed;
                case WorldGamepadInputType.RStickDown:
                    return gamepad.rightStick.down.isPressed;
                case WorldGamepadInputType.RStickLeft:
                    return gamepad.rightStick.left.isPressed;
                case WorldGamepadInputType.RStickRight:
                    return gamepad.rightStick.right.isPressed;
                case WorldGamepadInputType.A:
                    return gamepad.aButton.isPressed;
                case WorldGamepadInputType.B:
                    return gamepad.bButton.isPressed;
                case WorldGamepadInputType.X:
                    return gamepad.xButton.isPressed;
                case WorldGamepadInputType.Y:
                    return gamepad.yButton.isPressed;
                case WorldGamepadInputType.Start:
                    return gamepad.startButton.isPressed;
                case WorldGamepadInputType.Select:
                    return gamepad.selectButton.isPressed;
                case WorldGamepadInputType.LeftShoulder:
                    return gamepad.leftShoulder.isPressed;
                case WorldGamepadInputType.RightShoulder:
                    return gamepad.rightShoulder.isPressed;
                case WorldGamepadInputType.LeftTrigger:
                    return gamepad.leftTrigger.isPressed;
                case WorldGamepadInputType.RightTrigger:
                    return gamepad.rightTrigger.isPressed;
                case WorldGamepadInputType.DpadUp:
                    return gamepad.dpad.up.isPressed;
                case WorldGamepadInputType.DpadDown:
                    return gamepad.dpad.down.isPressed;
                case WorldGamepadInputType.DpadLeft:
                    return gamepad.dpad.left.isPressed;
                case WorldGamepadInputType.DpadRight:
                    return gamepad.dpad.right.isPressed;
                default:
                    return false;
            }
        }

    }

    public struct NullValue<T>
    {
        public T Value;
        public bool HasValue;

        public NullValue(T value)
        {
            Value = value;
            HasValue = true;
        }

        public void SetValue(T value)
        {
            Value = value;
            HasValue = true;
        }

    }
}