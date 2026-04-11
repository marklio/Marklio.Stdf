using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Marklio.Stdf.Generator;

/// <summary>
/// Analyzes the Roslyn syntax tree of types annotated with [StdfRecord]
/// to extract field metadata in declaration order.
/// </summary>
internal static class RecordAnalyzer
{
    public static RecordMetadata? Analyze(GeneratorAttributeSyntaxContext context)
    {
        var symbol = (INamedTypeSymbol)context.TargetSymbol;
        var attr = context.Attributes.FirstOrDefault(a =>
            a.AttributeClass?.Name == "StdfRecordAttribute");

        if (attr is null) return null;

        var recType = Convert.ToByte(attr.ConstructorArguments[0].Value);
        var recSub = Convert.ToByte(attr.ConstructorArguments[1].Value);

        var metadata = new RecordMetadata
        {
            Namespace = symbol.ContainingNamespace.ToDisplayString(),
            TypeName = symbol.Name,
            RecordType = recType,
            RecordSubType = recSub,
        };

        // Get properties in syntax-declaration order from the partial record struct
        var syntaxNode = (TypeDeclarationSyntax)context.TargetNode;
        int order = 0;

        foreach (var member in syntaxNode.Members)
        {
            if (member is not PropertyDeclarationSyntax propSyntax) continue;

            // Skip explicit interface implementations (e.g., ushort IBinRecord.BinNumber => ...)
            if (propSyntax.ExplicitInterfaceSpecifier != null) continue;

            var propSymbol = context.SemanticModel.GetDeclaredSymbol(propSyntax) as IPropertySymbol;
            if (propSymbol is null) continue;

            var field = AnalyzeProperty(propSymbol, order);
            if (field is not null)
            {
                metadata.Fields.Add(field);
                order++;
            }
        }

        return metadata;
    }

    private static FieldMetadata? AnalyzeProperty(IPropertySymbol prop, int order)
    {
        var field = new FieldMetadata
        {
            PropertyName = prop.Name,
            DeclarationOrder = order,
        };

        // Determine nullability and base type
        var type = prop.Type;
        if (type is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            field.IsNullable = true;
            type = nullable.TypeArguments[0];
        }
        else if (type.NullableAnnotation == NullableAnnotation.Annotated)
        {
            field.IsNullable = true;
        }

        // Check for array types
        bool isArray = type is IArrayTypeSymbol;
        ITypeSymbol elementType = isArray ? ((IArrayTypeSymbol)type).ElementType : type;

        field.ClrTypeName = elementType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        // Analyze attributes
        foreach (var attr in prop.GetAttributes())
        {
            var attrName = attr.AttributeClass?.Name;
            switch (attrName)
            {
                case "WireCountAttribute":
                    field.IsWireCount = true;
                    field.WireCountGroup = attr.ConstructorArguments[0].Value as string;
                    break;
                case "CountedArrayAttribute":
                    field.CountedArrayGroup = attr.ConstructorArguments[0].Value as string;
                    break;
                case "NibbleAttribute":
                    field.IsNibble = true;
                    break;
                case "BitArrayAttribute":
                    field.IsBitArray = true;
                    break;
                case "BitFieldAttribute":
                    field.IsBitField = true;
                    break;
                case "BitEncodedAttribute":
                    field.IsBitEncoded = true;
                    break;
                case "StdfDateTimeAttribute":
                    field.IsStdfDateTime = true;
                    break;
                case "C1Attribute":
                    field.IsC1 = true;
                    break;
                case "SnAttribute":
                    field.IsSn = true;
                    break;
                case "FixedStringAttribute":
                    field.FixedStringLength = (int)attr.ConstructorArguments[0].Value!;
                    break;
            }
        }

        // Resolve STDF type
        field.StdfType = ResolveStdfType(field, elementType, isArray);

        return field;
    }

    private static StdfFieldType ResolveStdfType(FieldMetadata field, ITypeSymbol elementType, bool isArray)
    {
        if (field.IsWireCount)
        {
            return elementType.SpecialType switch
            {
                SpecialType.System_Byte => StdfFieldType.WireCountU1,
                SpecialType.System_UInt16 => StdfFieldType.WireCountU2,
                SpecialType.System_UInt32 => StdfFieldType.WireCountU4,
                _ => StdfFieldType.WireCountU2,
            };
        }

        if (field.IsBitArray)
            return StdfFieldType.Dn;

        if (field.IsBitEncoded)
            return StdfFieldType.Bn;

        if (field.IsStdfDateTime)
            return StdfFieldType.DateTime;

        if (field.IsC1)
            return StdfFieldType.C1;

        if (field.IsSn)
            return isArray ? StdfFieldType.ArraySn : StdfFieldType.Sn;

        if (field.FixedStringLength > 0)
            return StdfFieldType.Cf;

        if (isArray)
        {
            if (field.IsNibble)
                return StdfFieldType.ArrayN1;

            return elementType.SpecialType switch
            {
                SpecialType.System_Byte => StdfFieldType.ArrayU1,
                SpecialType.System_UInt16 => StdfFieldType.ArrayU2,
                SpecialType.System_UInt32 => StdfFieldType.ArrayU4,
                SpecialType.System_UInt64 => StdfFieldType.ArrayU8,
                SpecialType.System_SByte => StdfFieldType.ArrayI1,
                SpecialType.System_Int16 => StdfFieldType.ArrayI2,
                SpecialType.System_Int32 => StdfFieldType.ArrayI4,
                SpecialType.System_Int64 => StdfFieldType.ArrayI8,
                SpecialType.System_Single => StdfFieldType.ArrayR4,
                SpecialType.System_Double => StdfFieldType.ArrayR8,
                SpecialType.System_String => StdfFieldType.ArrayCn,
                _ => StdfFieldType.ArrayU1,
            };
        }

        if (field.IsBitField)
            return StdfFieldType.B1;

        if (field.IsNibble)
            return StdfFieldType.N1;

        return elementType.SpecialType switch
        {
            SpecialType.System_Byte => StdfFieldType.U1,
            SpecialType.System_UInt16 => StdfFieldType.U2,
            SpecialType.System_UInt32 => StdfFieldType.U4,
            SpecialType.System_UInt64 => StdfFieldType.U8,
            SpecialType.System_SByte => StdfFieldType.I1,
            SpecialType.System_Int16 => StdfFieldType.I2,
            SpecialType.System_Int32 => StdfFieldType.I4,
            SpecialType.System_Int64 => StdfFieldType.I8,
            SpecialType.System_Single => StdfFieldType.R4,
            SpecialType.System_Double => StdfFieldType.R8,
            SpecialType.System_Char => StdfFieldType.C1,
            SpecialType.System_String => StdfFieldType.Cn,
            _ when elementType.Name == "DateTime" => StdfFieldType.DateTime,
            _ when elementType.Name == "BitArray" => StdfFieldType.Dn,
            _ => StdfFieldType.U1,
        };
    }
}
