﻿using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using EmptyKeys.UserInterface.Generator.Values;
using EmptyKeys.UserInterface.Generator.Values.Effects;

namespace EmptyKeys.UserInterface.Generator
{
    /// <summary>
    /// Implements generator for value types (values in resource dictionary, values for setters/triggers)
    /// </summary>
    public class ValueGenerator
    {
        /// <summary>
        /// Gets the generators.
        /// </summary>
        /// <value>
        /// The generators.
        /// </value>
        public Dictionary<Type, IGeneratorValue> Generators
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueGenerator"/> class.
        /// </summary>
        public ValueGenerator()
        {
            Generators = new Dictionary<Type, IGeneratorValue>();

            IGeneratorValue primitiveValue = new PrimitiveGeneratorValue<bool>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            primitiveValue = new PrimitiveGeneratorValue<byte>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            primitiveValue = new PrimitiveGeneratorValue<sbyte>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            primitiveValue = new PrimitiveGeneratorValue<short>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            primitiveValue = new PrimitiveGeneratorValue<ushort>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            primitiveValue = new PrimitiveGeneratorValue<int>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            primitiveValue = new PrimitiveGeneratorValue<uint>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            primitiveValue = new PrimitiveGeneratorValue<long>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            primitiveValue = new PrimitiveGeneratorValue<ulong>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            primitiveValue = new PrimitiveGeneratorValue<char>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            primitiveValue = new PrimitiveGeneratorValue<double>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            primitiveValue = new PrimitiveGeneratorValue<float>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            primitiveValue = new PrimitiveGeneratorValue<string>();
            Generators.Add(primitiveValue.ValueType, primitiveValue);

            IGeneratorValue brushValue = new BrushGeneratorValue<SolidColorBrush>();
            Generators.Add(brushValue.ValueType, brushValue);

            brushValue = new BrushGeneratorValue<LinearGradientBrush>();
            Generators.Add(brushValue.ValueType, brushValue);

            brushValue = new BrushGeneratorValue<ImageBrush>();
            Generators.Add(brushValue.ValueType, brushValue);

            IGeneratorValue bitmap = new BitmapImageGeneratorValue();
            Generators.Add(bitmap.ValueType, bitmap);

            IGeneratorValue resource = new ResourceExtGeneratorValue();
            Generators.Add(resource.ValueType, resource);

            IGeneratorValue dataTemplate = new DataTemplateGeneratorValue();
            Generators.Add(dataTemplate.ValueType, dataTemplate);

            IGeneratorValue controlTemplate = new ControlTemplateGeneratorValue();
            Generators.Add(controlTemplate.ValueType, controlTemplate);

            IGeneratorValue itemsPanel = new ItemsPanelTemplateGeneratorValue();
            Generators.Add(itemsPanel.ValueType, itemsPanel);

            IGeneratorValue style = new StyleGeneratorValue();
            Generators.Add(style.ValueType, style);

            IGeneratorValue sound = new SoundSourceGeneratorValue();
            Generators.Add(sound.ValueType, sound);

            IGeneratorValue thickness = new ThicknessGeneratorValue();
            Generators.Add(thickness.ValueType, thickness);

            IGeneratorValue doubleAnim = new DoubleAnimationGeneratorValue();
            Generators.Add(doubleAnim.ValueType, doubleAnim);

            IGeneratorValue brushAnim = new SolidColorBrushAnimGeneratorValue();
            Generators.Add(brushAnim.ValueType, brushAnim);

            IGeneratorValue thicknessAnim = new ThicknessAnimationGeneratorValue();
            Generators.Add(thicknessAnim.ValueType, thicknessAnim);

            IGeneratorValue rectangle = new RectangleGeometryGeneratorValue();
            Generators.Add(rectangle.ValueType, rectangle);

            IGeneratorValue ellipse = new EllipseGeometryGeneratorValue();
            Generators.Add(ellipse.ValueType, ellipse);

            IGeneratorValue line = new LineGeometryGeneratorValue();
            Generators.Add(line.ValueType, line);

            IGeneratorValue color = new ColorGeneratorValue();
            Generators.Add(color.ValueType, color);

            IGeneratorValue fontFamily = new FontFamilyGeneratorValue();
            Generators.Add(fontFamily.ValueType, fontFamily);

            IGeneratorValue fontWeight = new FontWeightGeneratorValue();
            Generators.Add(fontWeight.ValueType, fontWeight);

            IGeneratorValue directionalBlur = new DirectionalBlurEffectGeneratorValue();
            Generators.Add(directionalBlur.ValueType, directionalBlur);

            IGeneratorValue customEffect = new CustomEffectGeneratorValue();
            Generators.Add(customEffect.ValueType, customEffect);
        }

        /// <summary>
        /// Processes the generators.
        /// </summary>
        /// <param name="parentClass">The parent class.</param>
        /// <param name="method">The method.</param>
        /// <param name="value">The value.</param>
        /// <param name="baseName">Name of the base.</param>
        /// <param name="dictionary">The dictionary.</param>
        /// <returns></returns>
        public CodeExpression ProcessGenerators(CodeTypeDeclaration parentClass, CodeMemberMethod method, object value, string baseName, ResourceDictionary dictionary = null)
        {            
            if (value == null)
            {
                return new CodePrimitiveExpression(null);
            }

            IGeneratorValue generator;
            Type valueType = value.GetType();
            CodeExpression valueExpression = null;
            if (Generators.TryGetValue(valueType, out generator))
            {
                valueExpression = generator.Generate(parentClass, method, value, baseName, dictionary);
            }
            else if (valueType.IsEnum)
            {
                CodeTypeReferenceExpression typeReference = new CodeTypeReferenceExpression(valueType.Name);
                valueExpression = new CodeFieldReferenceExpression(typeReference, value.ToString());
            }
            else if (valueType.BaseType.Name.Contains("ViewModelLocatorBase"))
            {
                valueExpression = new CodeObjectCreateExpression(valueType);
            }
            else
            {
                //try reflection
                var gen = new ReflectionValueGenerator(valueType);
                Generators.Add(valueType, gen);
                valueExpression = gen.Generate(parentClass, method, value, baseName, dictionary);


                /*valueExpression = new CodePrimitiveExpression("NOT SUPPORTED!");
                string errorText = string.Format("Type {0} not supported", valueType.Name);
                Console.WriteLine(errorText);

                CodeSnippetStatement error = new CodeSnippetStatement("#error " + errorText);
                method.Statements.Add(error);*/
            }

            return valueExpression;
        }
    }

    internal class ReflectionValueGenerator : IGeneratorValue
    {

        internal ReflectionValueGenerator(Type forType)
        {
            ValueType = forType;
        }

        public Type ValueType
        {
            get;
            private set; 
        }

        public CodeExpression Generate(CodeTypeDeclaration parentClass, CodeMemberMethod method, object value, string baseName, ResourceDictionary dictionary = null)
        {
             var refExpr2 = new CodeObjectCreateExpression(ValueType,new CodeExpression[] { });
            CodeVariableDeclarationStatement variable = new CodeVariableDeclarationStatement(
                    ValueType, baseName, refExpr2
                    );
            method.Statements.Add(variable);

            var refExpr =  new CodeVariableReferenceExpression(baseName);

            foreach(var pi in ValueType.GetProperties())
            {
                var getM = pi.GetGetMethod();

                if (getM.GetParameters().Length == 0 && pi.GetSetMethod() != null)
                {
                    object o = getM.Invoke(value, new object[] { });
                    CodeComHelper.GenerateFieldNonGeneric(method, refExpr, pi.Name, o);
                }
            }



            return refExpr;
        }
    }

}
