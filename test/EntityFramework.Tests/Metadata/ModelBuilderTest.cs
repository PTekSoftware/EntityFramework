// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Metadata.ModelConventions;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class ModelBuilderTest
    {
        [Fact]
        public void Can_get_entity_builder_for_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            var entityBuilder = modelBuilder.Entity<Customer>();

            Assert.NotNull(entityBuilder);
            Assert.Equal(typeof(Customer).FullName, model.GetEntityType(typeof(Customer)).Name);
        }

        [Fact]
        public void Can_get_entity_builder_for_clr_type_non_generic()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            var entityBuilder = modelBuilder.Entity(typeof(Customer));

            Assert.NotNull(entityBuilder);
            Assert.Equal(typeof(Customer).FullName, model.GetEntityType(typeof(Customer)).Name);
        }

        [Fact]
        public void Can_get_entity_builder_for_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            var entityBuilder = modelBuilder.Entity(typeof(Customer).FullName);

            Assert.NotNull(entityBuilder);
            Assert.NotNull(model.TryGetEntityType(typeof(Customer).FullName));
        }

        [Fact]
        public void Cannot_get_entity_builder_for_ignored_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Ignore<Customer>();

            Assert.Equal(Strings.EntityIgnoredExplicitly(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity(typeof(Customer).FullName)).Message);
        }

        [Fact]
        public void Cannot_get_entity_builder_for_ignored_clr_type_non_generic()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Ignore(typeof(Customer));

            Assert.Equal(Strings.EntityIgnoredExplicitly(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity<Customer>()).Message);
        }

        [Fact]
        public void Cannot_get_entity_builder_for_ignored_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Ignore(typeof(Customer).FullName);

            Assert.Equal(Strings.EntityIgnoredExplicitly(typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity(typeof(Customer))).Message);
        }

        [Fact]
        public void Can_set_entity_key_from_clr_property()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>();

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_CLR_property_non_generic()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity(typeof(Customer), b => b.Key(Customer.IdProperty.Name));

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_property_name()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(Customer.IdProperty.Name);
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_property_name_when_no_clr_property()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property<int>(Customer.IdProperty.Name + 1);
                    b.Key(Customer.IdProperty.Name);
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Can_set_entity_key_from_property_name_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>(Customer.IdProperty.Name);
                    b.Key(Customer.IdProperty.Name);
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
        }

        [Fact]
        public void Setting_entity_key_from_property_name_when_no_property_throws()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            Assert.Equal(Strings.PropertyNotFound(Customer.IdProperty.Name, typeof(Customer).FullName),
                Assert.Throws<ModelItemNotFoundException>(() =>
                    modelBuilder.Entity(typeof(Customer).FullName, b => b.Key(Customer.IdProperty.Name))).Message);
        }

        [Fact]
        public void Setting_entity_key_from_clr_property_when_property_ignored_throws()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            Assert.Equal(Strings.PropertyIgnoredExplicitly(Customer.IdProperty.Name, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity<Customer>(b =>
                        {
                            b.Ignore(Customer.IdProperty.Name);
                            b.Key(e => e.Id);
                        })).Message);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_clr_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Key(e => new { e.Id, e.Name });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
            Assert.Equal(Customer.NameProperty.Name, entity.GetPrimaryKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_property_names()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Key(Customer.IdProperty.Name, Customer.NameProperty.Name);
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
            Assert.Equal(Customer.NameProperty.Name, entity.GetPrimaryKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_property_names_when_mixed_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Property<string>(Customer.NameProperty.Name + "Shadow");
                    b.Key(Customer.IdProperty.Name, Customer.NameProperty.Name + "Shadow");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
            Assert.Equal(Customer.NameProperty.Name + "Shadow", entity.GetPrimaryKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_composite_entity_key_from_property_names_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, ps =>
                {
                    ps.Property<int>(Customer.IdProperty.Name);
                    ps.Property<string>(Customer.NameProperty.Name);
                    ps.Key(Customer.IdProperty.Name, Customer.NameProperty.Name);
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(Customer.IdProperty.Name, entity.GetPrimaryKey().Properties.First().Name);
            Assert.Equal(Customer.NameProperty.Name, entity.GetPrimaryKey().Properties.Last().Name);
        }

        [Fact]
        public void Can_set_entity_key_with_annotations()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>(
                    b => b.Key(e => new { e.Id, e.Name })
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2"));

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(new[] { Customer.IdProperty.Name, Customer.NameProperty.Name }, entity.GetPrimaryKey().Properties.Select(p => p.Name));
            Assert.Equal("V1", entity.GetPrimaryKey()["A1"]);
            Assert.Equal("V2", entity.GetPrimaryKey()["A2"]);
        }

        [Fact]
        public void Can_set_entity_key_with_annotations_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>(Customer.IdProperty.Name);
                    b.Property<string>(Customer.NameProperty.Name);
                    b.Key(Customer.IdProperty.Name, Customer.NameProperty.Name)
                        .Annotation("A1", "V1")
                        .Annotation("A2", "V2");
                });

            var entity = model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entity.GetPrimaryKey().Properties.Count());
            Assert.Equal(new[] { Customer.IdProperty.Name, Customer.NameProperty.Name }, entity.GetPrimaryKey().Properties.Select(p => p.Name));
            Assert.Equal("V1", entity.GetPrimaryKey()["A1"]);
            Assert.Equal("V2", entity.GetPrimaryKey()["A2"]);
        }

        [Fact]
        public void Can_set_entity_annotation()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer))["foo"]);
        }

        [Fact]
        public void Can_set_entity_annotation_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity(typeof(Customer).FullName)
                .Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer))["foo"]);
        }

        [Fact]
        public void Can_set_property_annotation()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Property(c => c.Name).Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty(Customer.NameProperty.Name)["foo"]);
        }

        [Fact]
        public void Can_set_property_annotation_when_no_clr_property()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Property<string>(Customer.NameProperty.Name).Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty(Customer.NameProperty.Name)["foo"]);
        }

        [Fact]
        public void Can_set_property_annotation_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity(typeof(Customer).FullName)
                .Property<string>(Customer.NameProperty.Name).Annotation("foo", "bar");

            Assert.Equal("bar", model.GetEntityType(typeof(Customer)).GetProperty(Customer.NameProperty.Name)["foo"]);
        }

        [Fact]
        public void Can_add_multiple_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>();

            Assert.Equal(3, model.GetEntityType(typeof(Customer)).Properties.Count());
        }

        [Fact]
        public void Can_add_multiple_mixed_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>();

            Assert.Equal(3, model.GetEntityType(typeof(Customer)).Properties.Count());
        }

        [Fact]
        public void Can_add_multiple_properties_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>(Customer.IdProperty.Name);
                    b.Property<string>(Customer.NameProperty.Name).Annotation("foo", "bar");
                });

            Assert.Equal(2, model.GetEntityType(typeof(Customer)).Properties.Count());
        }

        [Fact]
        public void Properties_are_required_by_default_only_if_CLR_type_is_nullable()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up);
                    b.Property(e => e.Down);
                    b.Property<int>("Charm");
                    b.Property<string>("Strange");
                    b.Property(typeof(int), "Top");
                    b.Property(typeof(string), "Bottom");
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Up").IsNullable);
            Assert.True(entityType.GetProperty("Down").IsNullable);
            Assert.False(entityType.GetProperty("Charm").IsNullable);
            Assert.True(entityType.GetProperty("Strange").IsNullable);
            Assert.False(entityType.GetProperty("Top").IsNullable);
            Assert.True(entityType.GetProperty("Bottom").IsNullable);
        }

        [Fact]
        public void Properties_can_be_ignored()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            var entityType = (IEntityType)modelBuilder.Entity<Quarks>().Metadata;

            Assert.Equal(7, entityType.Properties.Count);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Ignore(e => e.Up);
                    b.Ignore(e => e.Down);
                    b.Ignore("Charm");
                    b.Ignore("Strange");
                    b.Ignore("Top");
                    b.Ignore("Bottom");
                    b.Ignore("Shadow");
                });

            Assert.Equal(Customer.IdProperty.Name, entityType.Properties.Single().Name);
        }

        [Fact]
        public void Ignoring_a_property_that_is_part_of_explicit_entity_key_throws()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            Assert.Equal(Strings.PropertyAddedExplicitly(Customer.IdProperty.Name, typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity<Customer>(b =>
                    {
                        b.Key(e => e.Id);
                        b.Ignore(e => e.Id);
                    })).Message);
        }

        [Fact]
        public void Ignoring_shadow_properties_when_they_have_been_added_throws()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            Assert.Equal(Strings.PropertyAddedExplicitly("Shadow", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity<Customer>(b =>
                    {
                        b.Property<string>("Shadow");
                        b.Ignore("Shadow");
                    })).Message);
        }

        [Fact]
        public void Adding_shadow_properties_when_they_have_been_ignored_throws()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            Assert.Equal(Strings.PropertyIgnoredExplicitly("Shadow", typeof(Customer).FullName),
                Assert.Throws<InvalidOperationException>(() =>
                    modelBuilder.Entity<Customer>(b =>
                        {
                            b.Ignore("Shadow");
                            b.Property<string>("Shadow");
                        })).Message);
        }

        [Fact]
        public void Properties_can_be_made_required()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).Required();
                    b.Property(e => e.Down).Required();
                    b.Property<int>("Charm").Required();
                    b.Property<string>("Strange").Required();
                    b.Property(typeof(int), "Top").Required();
                    b.Property(typeof(string), "Bottom").Required();
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Up").IsNullable);
            Assert.False(entityType.GetProperty("Down").IsNullable);
            Assert.False(entityType.GetProperty("Charm").IsNullable);
            Assert.False(entityType.GetProperty("Strange").IsNullable);
            Assert.False(entityType.GetProperty("Top").IsNullable);
            Assert.False(entityType.GetProperty("Bottom").IsNullable);
        }

        [Fact]
        public void Properties_can_be_made_optional()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
            {
                b.Property(e => e.Down).Required(false);
                b.Property<string>("Strange").Required(false);
                b.Property(typeof(string), "Bottom").Required(false);
            });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.True(entityType.GetProperty("Down").IsNullable);
            Assert.True(entityType.GetProperty("Strange").IsNullable);
            Assert.True(entityType.GetProperty("Bottom").IsNullable);
        }

        [Fact]
        public void Non_nullable_properties_cannot_be_made_optional()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
            {
                Assert.Equal(
                    Strings.CannotBeNullable("Up", "Quarks", "Int32"),
                    Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Up).Required(false)).Message);

                Assert.Equal(
                    Strings.CannotBeNullable("Charm", "Quarks", "Int32"),
                    Assert.Throws<InvalidOperationException>(() => b.Property<int>("Charm").Required(false)).Message);

                Assert.Equal(
                    Strings.CannotBeNullable("Top", "Quarks", "Int32"),
                    Assert.Throws<InvalidOperationException>(() => b.Property(typeof(int), "Top").Required(false)).Message);
            });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Up").IsNullable);
            Assert.False(entityType.GetProperty("Charm").IsNullable);
            Assert.False(entityType.GetProperty("Top").IsNullable);
        }

        [Fact]
        public void Properties_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property<int>("Charm");
                    b.Property(typeof(int), "Top");
                    b.Property<string>("Gluon");
                    b.Property(typeof(string), "Photon");
                });

            var entityType = model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty("Up").IsShadowProperty);
            Assert.False(entityType.GetProperty("Charm").IsShadowProperty);
            Assert.False(entityType.GetProperty("Top").IsShadowProperty);
            Assert.True(entityType.GetProperty("Gluon").IsShadowProperty);
            Assert.True(entityType.GetProperty("Photon").IsShadowProperty);

            Assert.Equal(-1, entityType.GetProperty("Up").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Charm").ShadowIndex);
            Assert.Equal(-1, entityType.GetProperty("Top").ShadowIndex);
            Assert.Equal(0, entityType.GetProperty("Gluon").ShadowIndex);
            Assert.Equal(1, entityType.GetProperty("Photon").ShadowIndex);
        }

        [Fact]
        public void Properties_can_be_made_shadow_properties_or_vice_versa()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).Shadow();
                    b.Property<int>("Charm").Shadow();
                    b.Property(typeof(int), "Top").Shadow();
                    b.Property<string>("Gluon").Shadow();
                    b.Property(typeof(string), "Photon").Shadow();
                });

            var entityType = model.GetEntityType(typeof(Quarks));

            Assert.True(entityType.GetProperty("Up").IsShadowProperty);
            Assert.True(entityType.GetProperty("Charm").IsShadowProperty);
            Assert.True(entityType.GetProperty("Top").IsShadowProperty);
            Assert.True(entityType.GetProperty("Gluon").IsShadowProperty);
            Assert.True(entityType.GetProperty("Photon").IsShadowProperty);

            Assert.Equal(4, entityType.GetProperty("Up").ShadowIndex);
            Assert.Equal(0, entityType.GetProperty("Charm").ShadowIndex);
            Assert.Equal(3, entityType.GetProperty("Top").ShadowIndex);
            Assert.Equal(1, entityType.GetProperty("Gluon").ShadowIndex);
            Assert.Equal(2, entityType.GetProperty("Photon").ShadowIndex);
        }

        [Fact]
        public void Properties_can_be_made_concurency_tokens()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).ConcurrencyToken();
                    b.Property(e => e.Down).ConcurrencyToken(false);
                    b.Property<int>("Charm").ConcurrencyToken(true);
                    b.Property<string>("Strange").ConcurrencyToken(false);
                    b.Property(typeof(int), "Top").ConcurrencyToken();
                    b.Property(typeof(string), "Bottom").ConcurrencyToken(false);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty(Customer.IdProperty.Name).IsConcurrencyToken);
            Assert.True(entityType.GetProperty("Up").IsConcurrencyToken);
            Assert.False(entityType.GetProperty("Down").IsConcurrencyToken);
            Assert.True(entityType.GetProperty("Charm").IsConcurrencyToken);
            Assert.False(entityType.GetProperty("Strange").IsConcurrencyToken);
            Assert.True(entityType.GetProperty("Top").IsConcurrencyToken);
            Assert.False(entityType.GetProperty("Bottom").IsConcurrencyToken);

            Assert.Equal(-1, entityType.GetProperty(Customer.IdProperty.Name).OriginalValueIndex);
            Assert.Equal(2, entityType.GetProperty("Up").OriginalValueIndex);
            Assert.Equal(-1, entityType.GetProperty("Down").OriginalValueIndex);
            Assert.Equal(0, entityType.GetProperty("Charm").OriginalValueIndex);
            Assert.Equal(-1, entityType.GetProperty("Strange").OriginalValueIndex);
            Assert.Equal(1, entityType.GetProperty("Top").OriginalValueIndex);
            Assert.Equal(-1, entityType.GetProperty("Bottom").OriginalValueIndex);
        }

        [Fact]
        public void Properties_can_be_set_to_generate_values_on_Add()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Id).GenerateValueOnAdd(false);
                    b.Property(e => e.Up).GenerateValueOnAdd();
                    b.Property(e => e.Down).GenerateValueOnAdd(true);
                    b.Property<int>("Charm").GenerateValueOnAdd();
                    b.Property<string>("Strange").GenerateValueOnAdd(false);
                    b.Property(typeof(int), "Top").GenerateValueOnAdd();
                    b.Property(typeof(string), "Bottom").GenerateValueOnAdd(false);
                });

            var entityType = model.GetEntityType(typeof(Quarks));

            Assert.Equal(false, entityType.GetProperty(Customer.IdProperty.Name).GenerateValueOnAdd);
            Assert.Equal(true, entityType.GetProperty("Up").GenerateValueOnAdd);
            Assert.Equal(true, entityType.GetProperty("Down").GenerateValueOnAdd);
            Assert.Equal(true, entityType.GetProperty("Charm").GenerateValueOnAdd);
            Assert.Equal(false, entityType.GetProperty("Strange").GenerateValueOnAdd);
            Assert.Equal(true, entityType.GetProperty("Top").GenerateValueOnAdd);
            Assert.Equal(false, entityType.GetProperty("Bottom").GenerateValueOnAdd);
        }

        [Fact]
        public void Properties_can_be_set_to_be_store_computed()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).StoreComputed();
                    b.Property(e => e.Down).StoreComputed(false);
                    b.Property<int>("Charm").StoreComputed();
                    b.Property<string>("Strange").StoreComputed(false);
                    b.Property(typeof(int), "Top").StoreComputed();
                    b.Property(typeof(string), "Bottom").StoreComputed(false);
                });

            var entityType = model.GetEntityType(typeof(Quarks));

            Assert.Null(entityType.GetProperty(Customer.IdProperty.Name).IsStoreComputed);
            Assert.Equal(true, entityType.GetProperty("Up").IsStoreComputed);
            Assert.Equal(false, entityType.GetProperty("Down").IsStoreComputed);
            Assert.Equal(true, entityType.GetProperty("Charm").IsStoreComputed);
            Assert.Equal(false, entityType.GetProperty("Strange").IsStoreComputed);
            Assert.Equal(true, entityType.GetProperty("Top").IsStoreComputed);
            Assert.Equal(false, entityType.GetProperty("Bottom").IsStoreComputed);
        }

        [Fact]
        public void Properties_can_be_set_to_use_store_default_values()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).UseStoreDefault();
                    b.Property(e => e.Down).UseStoreDefault(false);
                    b.Property<int>("Charm").UseStoreDefault();
                    b.Property<string>("Strange").UseStoreDefault(false);
                    b.Property(typeof(int), "Top").UseStoreDefault();
                    b.Property(typeof(string), "Bottom").UseStoreDefault(false);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.False(entityType.GetProperty(Customer.IdProperty.Name).UseStoreDefault);
            Assert.True(entityType.GetProperty("Up").UseStoreDefault);
            Assert.False(entityType.GetProperty("Down").UseStoreDefault);
            Assert.True(entityType.GetProperty("Charm").UseStoreDefault);
            Assert.False(entityType.GetProperty("Strange").UseStoreDefault);
            Assert.True(entityType.GetProperty("Top").UseStoreDefault);
            Assert.False(entityType.GetProperty("Bottom").UseStoreDefault);
        }

        [Fact]
        public void Can_set_max_length_for_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Quarks>(b =>
                {
                    b.Property(e => e.Up).MaxLength(0);
                    b.Property(e => e.Down).MaxLength(100);
                    b.Property<int>("Charm").MaxLength(0);
                    b.Property<string>("Strange").MaxLength(100);
                    b.Property(typeof(int), "Top").MaxLength(0);
                    b.Property(typeof(string), "Bottom").MaxLength(100);
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Quarks));

            Assert.Equal(0, entityType.GetProperty(Customer.IdProperty.Name).MaxLength);
            Assert.Equal(0, entityType.GetProperty("Up").MaxLength);
            Assert.Equal(100, entityType.GetProperty("Down").MaxLength);
            Assert.Equal(0, entityType.GetProperty("Charm").MaxLength);
            Assert.Equal(100, entityType.GetProperty("Strange").MaxLength);
            Assert.Equal(0, entityType.GetProperty("Top").MaxLength);
            Assert.Equal(100, entityType.GetProperty("Bottom").MaxLength);
        }

        [Fact]
        public void PropertyBuilder_methods_can_be_chained()
        {
            new ModelBuilder()
                .Entity<Quarks>()
                .Property(e => e.Up)
                .Required()
                .Annotation("A", "V")
                .ConcurrencyToken()
                .Shadow()
                .StoreComputed()
                .GenerateValueOnAdd()
                .UseStoreDefault()
                .MaxLength(100)
                .Required();
        }

        [Fact]
        public void Can_add_foreign_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(1, entityType.ForeignKeys.Count());
        }

        [Fact]
        public void Can_add_foreign_key_when_no_clr_property()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>();

            modelBuilder.Entity<Order>(b => b.ForeignKey(typeof(Customer).FullName, new[] { "CustomerId" }));

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(1, entityType.ForeignKeys.Count());
        }

        [Fact]
        public void Can_add_foreign_key_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>();

            modelBuilder.Entity(typeof(Order).FullName, b =>
                {
                    b.Property<int>("CustomerId");
                    b.ForeignKey(typeof(Customer).FullName, "CustomerId");
                });

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(1, entityType.ForeignKeys.Count());
        }

        [Fact]
        public void Can_add_foreign_key_when_no_clr_type_on_both_ends()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>(Customer.IdProperty.Name);
                    b.Key(new[] { Customer.IdProperty.Name });
                });

            modelBuilder.Entity(typeof(Order).FullName, b =>
                {
                    b.Property<int>("CustomerId");
                    b.ForeignKey(typeof(Customer).FullName, "CustomerId");
                });

            var entityType = model.GetEntityType(typeof(Order));

            Assert.Equal(1, entityType.ForeignKeys.Count());
        }

        [Fact]
        public void Can_add_multiple_foreign_keys()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>();

            modelBuilder.Entity<Order>(b =>
                {
                    b.ForeignKey<Customer>(c => c.CustomerId);
                    b.ForeignKey<Customer>(c => c.AnotherCustomerId).IsUnique();
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Order));

            Assert.Equal(2, entityType.ForeignKeys.Count());
            Assert.True(entityType.ForeignKeys.Last().IsUnique);
        }

        [Fact]
        public void Can_add_multiple_foreign_keys_when_mixed_properties()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>();

            modelBuilder.Entity<Order>(b =>
                {
                    b.ForeignKey<Customer>(c => c.CustomerId);
                    b.ForeignKey(typeof(Customer).FullName, "AnotherCustomerId").IsUnique();
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Order));

            Assert.Equal(2, entityType.ForeignKeys.Count());
            Assert.True(entityType.ForeignKeys.Last().IsUnique);
        }

        [Fact]
        public void Can_add_multiple_foreign_keys_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>();

            modelBuilder
                .Entity(typeof(Order).FullName, b =>
                    {
                        b.Property<int>("CustomerId");
                        b.Property<int>("AnotherCustomerId");
                        b.ForeignKey(typeof(Customer).FullName, "CustomerId");
                        b.ForeignKey(typeof(Customer).FullName, "AnotherCustomerId").IsUnique();
                    });

            var entityType = (IEntityType)model.GetEntityType(typeof(Order).FullName);

            Assert.Equal(2, entityType.ForeignKeys.Count());
            Assert.True(entityType.ForeignKeys.Last().IsUnique);
        }

        [Fact]
        public void Can_add_multiple_foreign_keys_when_no_clr_type_on_both_ends()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>(Customer.IdProperty.Name);
                    b.Key(Customer.IdProperty.Name);
                });

            modelBuilder.Entity(typeof(Order).FullName, b =>
                {
                    b.Property<int>("CustomerId");
                    b.Property<int>("AnotherCustomerId");
                    b.ForeignKey(typeof(Customer).FullName, "CustomerId");
                    b.ForeignKey(typeof(Customer).FullName, "AnotherCustomerId").IsUnique();
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Order));

            Assert.Equal(2, entityType.ForeignKeys.Count());
            Assert.True(entityType.ForeignKeys.Last().IsUnique);
        }

        [Fact]
        public void Can_add_index()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Customer>()
                .Index(ix => ix.Name);

            var entityType = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entityType.Indexes.Count());
        }

        [Fact]
        public void Can_add_index_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<string>(Customer.NameProperty.Name);
                    b.Index(Customer.NameProperty.Name);
                });

            var entityType = model.GetEntityType(typeof(Customer));

            Assert.Equal(1, entityType.Indexes.Count());
        }

        [Fact]
        public void Can_add_multiple_indexes()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity<Customer>(b =>
                {
                    b.Index(ix => ix.Id).IsUnique();
                    b.Index(ix => ix.Name).Annotation("A1", "V1");
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entityType.Indexes.Count());
            Assert.True(entityType.Indexes.First().IsUnique);
            Assert.False(entityType.Indexes.Last().IsUnique);
            Assert.Equal("V1", entityType.Indexes.Last()["A1"]);
        }

        [Fact]
        public void Can_add_multiple_indexes_when_no_clr_type()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder.Entity(typeof(Customer).FullName, b =>
                {
                    b.Property<int>(Customer.IdProperty.Name);
                    b.Property<string>(Customer.NameProperty.Name);
                    b.Index(Customer.IdProperty.Name).IsUnique();
                    b.Index(Customer.NameProperty.Name).Annotation("A1", "V1");
                });

            var entityType = (IEntityType)model.GetEntityType(typeof(Customer));

            Assert.Equal(2, entityType.Indexes.Count());
            Assert.True(entityType.Indexes.First().IsUnique);
            Assert.False(entityType.Indexes.Last().IsUnique);
            Assert.Equal("V1", entityType.Indexes.Last()["A1"]);
        }

        [Fact]
        public void OneToMany_finds_existing_navs_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navToPrincipal = dependentType.AddNavigation("Customer", fk, pointsToPrincipal: true);
            var navToDependent = principalType.AddNavigation("Orders", fk, pointsToPrincipal: false);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToMany(e => e.Orders, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navToPrincipal, dependentType.Navigations.Single());
            Assert.Same(navToDependent, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_finds_existing_nav_to_principal_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = dependentType.AddNavigation("Customer", fk, pointsToPrincipal: true);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToMany(e => e.Orders, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navigation, dependentType.Navigations.Single());
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_finds_existing_nav_to_dependent_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = principalType.AddNavigation("Orders", fk, pointsToPrincipal: false);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToMany(e => e.Orders, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Same(navigation, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_existing_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToMany(e => e.Orders, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToMany(e => e.Orders, e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToMany(e => e.Orders);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            // Passing null as the first arg is not super-compelling, but it is consistent
            modelBuilder.Entity<Customer>().OneToMany<Order>(null, e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToMany<Order>();

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_specified_FK_even_if_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_existing_FK_not_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder
                .Entity<Pickle>()
                .ForeignKey<BigMak>(c => c.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToMany(e => e.Pickles, e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToMany(e => e.Pickles, e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToMany(e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToMany<Pickle>(null, e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToMany<Pickle>()
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<BigMak>().OneToMany(e => e.Pickles, e => e.BigMak);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<BigMak>().OneToMany(e => e.Pickles);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<BigMak>().OneToMany<Pickle>(null, e => e.BigMak);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<BigMak>().OneToMany<Pickle>();

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_matches_shadow_FK_property_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>().Property<int>("BigMakId");

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            var fkProperty = dependentType.GetProperty("BigMakId");

            modelBuilder.Entity<BigMak>().OneToMany(e => e.Pickles, e => e.BigMak);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_shadow_FK_when_uniqueness_does_not_match()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder
                .Entity<Pickle>()
                .ForeignKey<BigMak>(c => c.BurgerId)
                .IsUnique();

            var dependentType = (IEntityType)model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToMany(e => e.Pickles, e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            Assert.Equal(2, dependentType.ForeignKeys.Count);
            var newFk = dependentType.ForeignKeys.Single(k => k != fk);

            Assert.True(fk.IsUnique);
            Assert.False(newFk.IsUnique);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_use_explicitly_specified_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ReferencedKey(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_use_non_PK_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_both_convention_properties_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForeignKey(e => e.CustomerId)
                .ReferencedKey(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_both_convention_properties_specified_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ReferencedKey(e => e.Id)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_FK_by_convention_specified_with_explicit_principal_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForeignKey(e => e.CustomerId)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_FK_by_convention_specified_with_explicit_principal_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ReferencedKey(e => e.AlternateKey)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_principal_key_by_convention_specified_with_explicit_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToMany(e => e.Pickles, e => e.BigMak)
                .ForeignKey(e => e.BurgerId)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_have_principal_key_by_convention_specified_with_explicit_PK_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToMany(e => e.Pickles, e => e.BigMak)
                .ReferencedKey(e => e.AlternateKey)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_finds_existing_navs_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navToPrincipal = dependentType.AddNavigation("Customer", fk, pointsToPrincipal: true);
            var navToDependent = principalType.AddNavigation("Orders", fk, pointsToPrincipal: false);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().ManyToOne(e => e.Customer, e => e.Orders);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navToPrincipal, dependentType.Navigations.Single());
            Assert.Same(navToDependent, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_finds_existing_nav_to_principal_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = dependentType.AddNavigation("Customer", fk, pointsToPrincipal: true);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().ManyToOne(e => e.Customer, e => e.Orders);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navigation, dependentType.Navigations.Single());
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_finds_existing_nav_to_dependent_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var navigation = principalType.AddNavigation("Orders", fk, pointsToPrincipal: false);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().ManyToOne(e => e.Customer, e => e.Orders);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Same(navigation, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_existing_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(c => c.CustomerId);

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().ManyToOne(e => e.Customer, e => e.Orders);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().ManyToOne(e => e.Customer, e => e.Orders);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            // Passing null as the first arg is not super-compelling, but it is consistent
            modelBuilder.Entity<Order>().ManyToOne<Customer>(null, e => e.Orders);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().ManyToOne(e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().ManyToOne<Customer>();

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_specified_FK_even_if_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_existing_FK_not_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder
                .Entity<Pickle>()
                .ForeignKey<BigMak>(c => c.BurgerId);

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne(e => e.BigMak, e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne(e => e.BigMak, e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne<BigMak>(null, e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne(e => e.BigMak)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne<BigMak>()
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Pickle>().ManyToOne(e => e.BigMak, e => e.Pickles);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Pickle>().ManyToOne<BigMak>(null, e => e.Pickles);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Pickle>().ManyToOne(e => e.BigMak);

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations_with_shadow_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Pickle>().ManyToOne<BigMak>();

            var fk = dependentType.ForeignKeys.Single();
            var fkProperty = fk.Properties.Single();

            Assert.Equal("BigMakId", fkProperty.Name);
            Assert.True(fkProperty.IsShadowProperty);
            Assert.Same(typeof(int?), fkProperty.PropertyType);
            Assert.Same(dependentType, fkProperty.EntityType);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_matches_shadow_FK_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>().Property<int>("BigMakId");

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            var fkProperty = dependentType.GetProperty("BigMakId");

            modelBuilder.Entity<Pickle>().ManyToOne(e => e.BigMak, e => e.Pickles);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_shadow_FK_if_uniqueness_does_not_match()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder
                .Entity<Pickle>()
                .ForeignKey<BigMak>(c => c.BurgerId)
                .IsUnique();

            var dependentType = (IEntityType)model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne(e => e.BigMak, e => e.Pickles)
                .ForeignKey(e => e.BurgerId);

            Assert.Equal(2, dependentType.ForeignKeys.Count);
            var newFk = dependentType.ForeignKeys.Single(k => k != fk);

            Assert.True(fk.IsUnique);
            Assert.False(newFk.IsUnique);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_use_explicitly_specified_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ReferencedKey(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_use_non_PK_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_both_convention_properties_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForeignKey(e => e.CustomerId)
                .ReferencedKey(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_both_convention_properties_specified_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ReferencedKey(e => e.Id)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_FK_by_convention_specified_with_explicit_principal_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForeignKey(e => e.CustomerId)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_FK_by_convention_specified_with_explicit_principal_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<Order>();

            var dependentType = model.GetEntityType(typeof(Order));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty("CustomerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ReferencedKey(e => e.AlternateKey)
                .ForeignKey(e => e.CustomerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Orders", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_principal_key_by_convention_specified_with_explicit_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne(e => e.BigMak, e => e.Pickles)
                .ForeignKey(e => e.BurgerId)
                .ReferencedKey(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_have_principal_key_by_convention_specified_with_explicit_PK_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Pickle>();

            var dependentType = model.GetEntityType(typeof(Pickle));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Pickle>()
                .ManyToOne(e => e.BigMak, e => e.Pickles)
                .ReferencedKey(e => e.AlternateKey)
                .ForeignKey(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Pickles", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_finds_existing_navs_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<CustomerDetails>()
                .ForeignKey<Customer>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var navToPrincipal = dependentType.AddNavigation("Customer", fk, pointsToPrincipal: true);
            var navToDependent = principalType.AddNavigation("Details", fk, pointsToPrincipal: false);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToOne(e => e.Details, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navToPrincipal, dependentType.Navigations.Single());
            Assert.Same(navToDependent, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_finds_existing_nav_to_principal_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<CustomerDetails>()
                .ForeignKey<Customer>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var navigation = dependentType.AddNavigation("Customer", fk, pointsToPrincipal: true);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToOne(e => e.Details, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Same(navigation, dependentType.Navigations.Single());
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_finds_existing_nav_to_dependent_and_uses_associated_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder
                .Entity<CustomerDetails>()
                .ForeignKey<Customer>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var navigation = principalType.AddNavigation("Details", fk, pointsToPrincipal: false);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToOne(e => e.Details, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Same(navigation, principalType.Navigations.Single());
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();
            modelBuilder
                .Entity<CustomerDetails>()
                .ForeignKey<Customer>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToOne(e => e.Details, e => e.Customer);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToOne(e => e.Details, e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_FK_when_not_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder
                .Entity<OrderDetails>()
                .ForeignKey<Order>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().OneToOne(e => e.Details, e => e.Order);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_new_FK_when_not_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Order>().OneToOne(e => e.Details, e => e.Order);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToOne(e => e.Details);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_from_other_end_nav_and_new_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            // Passing null as the first arg is not super-compelling, but it is consistent
            modelBuilder.Entity<Customer>().OneToOne<CustomerDetails>(null, e => e.Customer);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_relationship_with_no_navigations()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder.Entity<Customer>().OneToOne<CustomerDetails>();

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_specified_FK_even_if_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
                .ForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_specified_FK_even_if_PK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>()
                .OneToOne(e => e.Details, e => e.Customer)
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_FK_not_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder
                .Entity<Bun>()
                .ForeignKey<BigMak>(c => c.BurgerId);

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToOne(e => e.Bun, e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToOne(e => e.Bun, e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToOne(e => e.Bun)
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_from_other_end_nav_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToOne<Bun>(null, e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_relationship_with_no_navigations_and_specified_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToOne<Bun>()
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_shadow_FK_when_uniqueness_does_not_match()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder
                .Entity<Bun>()
                .ForeignKey<BigMak>(c => c.BurgerId);

            var dependentType = (IEntityType)model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToOne(e => e.Bun, e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId);

            Assert.Equal(2, dependentType.ForeignKeys.Count);
            var newFk = dependentType.ForeignKeys.Single(k => k != fk);

            Assert.False(fk.IsUnique);
            Assert.True(newFk.IsUnique);

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(newFk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(newFk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount + 1, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_with_existing_FK_still_used()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder
                .Entity<OrderDetails>()
                .ForeignKey<Order>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<OrderDetails>()
                .OneToOne(e => e.Order, e => e.Details)
                .ForeignKey<OrderDetails>(e => e.Id);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_with_FK_still_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<OrderDetails>()
                .OneToOne(e => e.Order, e => e.Details)
                .ForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_from_other_end_OneToOne_principal_and_dependent_can_be_flipped()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne<Customer>(e => e.Customer)
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_OneToOne_principal_and_dependent_can_be_flipped()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne<Customer>(null, e => e.Details)
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void No_navigation_OneToOne_principal_and_dependent_can_be_flipped()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne<Customer>()
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_with_PK_FK_still_used()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne(e => e.Customer, e => e.Details)
                .ForeignKey<CustomerDetails>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_PK_explicitly_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);
            var principalProperty = principalType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>()
                .OneToOne(e => e.Details, e => e.Customer)
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_use_alternate_principal_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Customer>()
                .OneToOne(e => e.Details, e => e.Customer)
                .ReferencedKey<Customer>(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_both_convention_keys_specified_explicitly()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");
            var principalProperty = principalType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
                .ForeignKey<OrderDetails>(e => e.OrderId)
                .ReferencedKey<Order>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_both_convention_keys_specified_explicitly_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");
            var principalProperty = principalType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Order>()
                .OneToOne(e => e.Details, e => e.Order)
                .ReferencedKey<Order>(e => e.OrderId)
                .ForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_both_keys_specified_explicitly()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToOne(e => e.Bun, e => e.BigMak)
                .ForeignKey<Bun>(e => e.BurgerId)
                .ReferencedKey<BigMak>(e => e.AlternateKey);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_have_both_keys_specified_explicitly_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<BigMak>();
            modelBuilder.Entity<Bun>();

            var dependentType = model.GetEntityType(typeof(Bun));
            var principalType = model.GetEntityType(typeof(BigMak));

            var fkProperty = dependentType.GetProperty("BurgerId");
            var principalProperty = principalType.GetProperty("AlternateKey");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<BigMak>()
                .OneToOne(e => e.Bun, e => e.BigMak)
                .ReferencedKey<BigMak>(e => e.AlternateKey)
                .ForeignKey<Bun>(e => e.BurgerId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());
            Assert.Same(principalProperty, fk.ReferencedProperties.Single());

            Assert.Equal("BigMak", dependentType.Navigations.Single().Name);
            Assert.Equal("Bun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_using_principal_with_existing_FK_still_used()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();
            modelBuilder
                .Entity<OrderDetails>()
                .ForeignKey<Order>(c => c.Id);

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<OrderDetails>()
                .OneToOne(e => e.Order, e => e.Details)
                .ReferencedKey<Order>(e => e.OrderId);

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_using_principal_with_FK_still_found_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<OrderDetails>()
                .OneToOne(e => e.Order, e => e.Details)
                .ReferencedKey<Order>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_in_both_ways()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<OrderDetails>()
                .OneToOne(e => e.Order, e => e.Details)
                .ForeignKey<OrderDetails>(e => e.OrderId)
                .ReferencedKey<Order>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_principal_and_dependent_can_be_flipped_in_both_ways_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Order>();
            modelBuilder.Entity<OrderDetails>();

            var dependentType = model.GetEntityType(typeof(OrderDetails));
            var principalType = model.GetEntityType(typeof(Order));

            var fkProperty = dependentType.GetProperty("OrderId");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<OrderDetails>()
                .OneToOne(e => e.Order, e => e.Details)
                .ReferencedKey<Order>(e => e.OrderId)
                .ForeignKey<OrderDetails>(e => e.OrderId);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Order", dependentType.Navigations.Single().Name);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_from_other_end_OneToOne_principal_and_dependent_can_be_flipped_using_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne<Customer>(e => e.Customer)
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Equal("Customer", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Unidirectional_OneToOne_principal_and_dependent_can_be_flipped_using_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne<Customer>(null, e => e.Details)
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Details", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void No_navigation_OneToOne_principal_and_dependent_can_be_flipped_using_principal()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Customer>();
            modelBuilder.Entity<CustomerDetails>();

            var dependentType = model.GetEntityType(typeof(CustomerDetails));
            var principalType = model.GetEntityType(typeof(Customer));

            var fkProperty = dependentType.GetProperty(Customer.IdProperty.Name);

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<CustomerDetails>()
                .OneToOne<Customer>()
                .ReferencedKey<Customer>(e => e.Id);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty, fk.Properties.Single());

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        private class BigMak
        {
            public int Id { get; set; }
            public int AlternateKey { get; set; }

            public IEnumerable<Pickle> Pickles { get; set; }

            public Bun Bun { get; set; }
        }

        private class Pickle
        {
            public int Id { get; set; }

            public int BurgerId { get; set; }
            public BigMak BigMak { get; set; }
        }

        private class Bun
        {
            public int Id { get; set; }

            public int BurgerId { get; set; }
            public BigMak BigMak { get; set; }
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_uses_existing_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder
                .Entity<Tomato>()
                .ForeignKey<Whoopper>(c => new { c.BurgerId1, c.BurgerId2 });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany(e => e.Tomatoes, e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_creates_both_navs_and_creates_composite_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany(e => e.Tomatoes, e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_use_alternate_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany(e => e.Tomatoes, e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 })
                .ReferencedKey(e => new { e.AlternateKey1, e.AlternateKey2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_use_alternate_composite_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany(e => e.Tomatoes, e => e.Whoopper)
                .ReferencedKey(e => new { e.AlternateKey1, e.AlternateKey2 })
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>(b =>
                {
                    b.Property(e => e.BurgerId1);
                    b.Property(e => e.BurgerId2);
                });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany(e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_unidirectional_from_other_end_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany<Tomato>(null, e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToMany_can_create_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToMany<Tomato>()
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_uses_existing_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder
                .Entity<Tomato>()
                .ForeignKey<Whoopper>(c => new { c.BurgerId1, c.BurgerId2 });

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var fk = dependentType.ForeignKeys.Single();

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne(e => e.Whoopper, e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_creates_both_navs_and_creates_composite_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne(e => e.Whoopper, e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_use_alternate_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne(e => e.Whoopper, e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 })
                .ReferencedKey(e => new { e.AlternateKey1, e.AlternateKey2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_use_alternate_composite_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<Tomato>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne(e => e.Whoopper, e => e.Tomatoes)
                .ReferencedKey(e => new { e.AlternateKey1, e.AlternateKey2 })
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne<Whoopper>(null, e => e.Tomatoes)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("Tomatoes", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_unidirectional_from_other_end_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne(e => e.Whoopper)
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void ManyToOne_can_create_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Tomato>();

            var dependentType = model.GetEntityType(typeof(Tomato));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Tomato>()
                .ManyToOne<Whoopper>()
                .ForeignKey(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_uses_existing_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder
                .Entity<ToastedBun>()
                .ForeignKey<Whoopper>(c => new { c.BurgerId1, c.BurgerId2 });

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var fk = dependentType.ForeignKeys.Single();
            fk.IsUnique = true;

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne(e => e.ToastedBun, e => e.Whoopper)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            Assert.Same(fk, dependentType.ForeignKeys.Single());
            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_creates_both_navs_and_creates_composite_FK_specified()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>();

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne(e => e.ToastedBun, e => e.Whoopper)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_use_alternate_composite_key()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<ToastedBun>();

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne(e => e.ToastedBun, e => e.Whoopper)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 })
                .ReferencedKey<Whoopper>(e => new { e.AlternateKey1, e.AlternateKey2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_use_alternate_composite_key_in_any_order()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>(b => b.Key(c => new { c.Id1, c.Id2 }));
            modelBuilder.Entity<ToastedBun>();

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));
            var principalProperty1 = principalType.GetProperty("AlternateKey1");
            var principalProperty2 = principalType.GetProperty("AlternateKey2");

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne(e => e.ToastedBun, e => e.Whoopper)
                .ReferencedKey<Whoopper>(e => new { e.AlternateKey1, e.AlternateKey2 })
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);
            Assert.Same(principalProperty1, fk.ReferencedProperties[0]);
            Assert.Same(principalProperty2, fk.ReferencedProperties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);

            Assert.Equal(2, principalType.Keys.Count);
            Assert.Contains(principalKey, principalType.Keys);
            Assert.Contains(fk.ReferencedKey, principalType.Keys);
            Assert.NotSame(principalKey, fk.ReferencedKey);

            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_uses_composite_PK_for_FK_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });

            var dependentType = model.GetEntityType(typeof(Moostard));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("Id1");
            var fkProperty2 = dependentType.GetProperty("Id2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne(e => e.Moostard, e => e.Whoopper);

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Moostard", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_be_flipped_and_composite_PK_is_still_used_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });

            var dependentType = model.GetEntityType(typeof(Moostard));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("Id1");
            var fkProperty2 = dependentType.GetProperty("Id2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Moostard>()
                .OneToOne(e => e.Whoopper, e => e.Moostard)
                .ForeignKey<Moostard>(e => new { e.Id1, e.Id2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Moostard", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_be_flipped_using_principal_and_composite_PK_is_still_used_by_convention()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<Moostard>().Key(c => new { c.Id1, c.Id2 });

            var dependentType = model.GetEntityType(typeof(Moostard));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("Id1");
            var fkProperty2 = dependentType.GetProperty("Id2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Moostard>()
                .OneToOne(e => e.Whoopper, e => e.Moostard)
                .ReferencedKey<Whoopper>(e => new { e.Id1, e.Id2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Equal("Moostard", principalType.Navigations.Single().Name);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>();

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne(e => e.ToastedBun)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Equal("ToastedBun", principalType.Navigations.Single().Name);
            Assert.Same(fk, principalType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_unidirectional_from_other_end_nav_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>();

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne<ToastedBun>(null, e => e.Whoopper)
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Equal("Whoopper", dependentType.Navigations.Single().Name);
            Assert.Empty(principalType.Navigations);
            Assert.Same(fk, dependentType.Navigations.Single().ForeignKey);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void OneToOne_can_create_relationship_with_no_navigations_and_specified_composite_FK()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);
            modelBuilder.Entity<Whoopper>().Key(c => new { c.Id1, c.Id2 });
            modelBuilder.Entity<ToastedBun>();

            var dependentType = model.GetEntityType(typeof(ToastedBun));
            var principalType = model.GetEntityType(typeof(Whoopper));

            var fkProperty1 = dependentType.GetProperty("BurgerId1");
            var fkProperty2 = dependentType.GetProperty("BurgerId2");

            var principalPropertyCount = principalType.Properties.Count;
            var dependentPropertyCount = dependentType.Properties.Count;
            var principalKey = principalType.Keys.Single();
            var dependentKey = dependentType.Keys.Single();

            modelBuilder
                .Entity<Whoopper>()
                .OneToOne<ToastedBun>()
                .ForeignKey<ToastedBun>(e => new { e.BurgerId1, e.BurgerId2 });

            var fk = dependentType.ForeignKeys.Single();
            Assert.Same(fkProperty1, fk.Properties[0]);
            Assert.Same(fkProperty2, fk.Properties[1]);

            Assert.Empty(dependentType.Navigations);
            Assert.Empty(principalType.Navigations);
            Assert.Equal(principalPropertyCount, principalType.Properties.Count);
            Assert.Equal(dependentPropertyCount, dependentType.Properties.Count);
            Assert.Empty(principalType.ForeignKeys);
            Assert.Same(principalKey, principalType.Keys.Single());
            Assert.Same(dependentKey, dependentType.Keys.Single());
            Assert.Same(principalKey, principalType.GetPrimaryKey());
            Assert.Same(dependentKey, dependentType.GetPrimaryKey());
        }

        [Fact]
        public void Can_convert_to_non_convention_builder()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            Assert.Same(model, new BasicModelBuilder(modelBuilder.Model).Model);
        }

        private class Whoopper
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }
            public int AlternateKey1 { get; set; }
            public int AlternateKey2 { get; set; }

            public IEnumerable<Tomato> Tomatoes { get; set; }

            public ToastedBun ToastedBun { get; set; }

            public Moostard Moostard { get; set; }
        }

        private class Tomato
        {
            public int Id { get; set; }

            public int BurgerId1 { get; set; }
            public int BurgerId2 { get; set; }
            public Whoopper Whoopper { get; set; }
        }

        private class ToastedBun
        {
            public int Id { get; set; }

            public int BurgerId1 { get; set; }
            public int BurgerId2 { get; set; }
            public Whoopper Whoopper { get; set; }
        }

        private class Moostard
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }

            public Whoopper Whoopper { get; set; }
        }

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");
            public static readonly PropertyInfo NameProperty = typeof(Customer).GetProperty("Name");

            public int Id { get; set; }
            public int AlternateKey { get; set; }
            public string Name { get; set; }

            public IEnumerable<Order> Orders { get; set; }

            public CustomerDetails Details { get; set; }
        }

        private class CustomerDetails
        {
            public int Id { get; set; }

            public Customer Customer { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }

            public int? CustomerId { get; set; }
            public int AnotherCustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderDetails Details { get; set; }
        }

        private class OrderDetails
        {
            public int Id { get; set; }

            public int OrderId { get; set; }
            public Order Order { get; set; }
        }

        // INotify interfaces not really implemented; just marking the classes to test metadata construction
        private class Quarks : INotifyPropertyChanging, INotifyPropertyChanged
        {
            public int Id { get; set; }

            public int Up { get; set; }
            public string Down { get; set; }
            private int Charm { get; set; }
            private string Strange { get; set; }
            private int Top { get; set; }
            private string Bottom { get; set; }

#pragma warning disable 67
            public event PropertyChangingEventHandler PropertyChanging;
            public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
        }

        [Fact]
        public void One_to_many_relationships_with_nullable_keys_are_optional_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>()
                .OneToMany(e => e.Nobs, e => e.Hob)
                .ForeignKey(e => new { e.HobId1, e.HobId2 });

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_many_relationships_with_non_nullable_keys_are_required_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Nob>()
                .OneToMany(e => e.Hobs, e => e.Nob)
                .ForeignKey(e => new { e.NobId1, e.NobId2 });

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void Many_to_one_relationships_with_nullable_keys_are_optional_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Nob>()
                .ManyToOne(e => e.Hob, e => e.Nobs)
                .ForeignKey(e => new { e.HobId1, e.HobId2 });

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void Many_to_one_relationships_with_non_nullable_keys_are_required_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>()
                .ManyToOne(e => e.Nob, e => e.Hobs)
                .ForeignKey(e => new { e.NobId1, e.NobId2 });

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_with_nullable_keys_are_optional_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>()
                .OneToOne(e => e.Nob, e => e.Hob)
                .ForeignKey<Nob>(e => new { e.HobId1, e.HobId2 });

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_with_non_nullable_keys_are_required_by_default()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Nob>()
                .OneToOne(e => e.Hob, e => e.Nob)
                .ForeignKey<Hob>(e => new { e.NobId1, e.NobId2 });

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_many_relationships_with_nullable_keys_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>()
                .OneToMany(e => e.Nobs, e => e.Hob)
                .ForeignKey(e => new { e.HobId1, e.HobId2 })
                .Required();

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_many_relationships_with_non_nullable_keys_cannot_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();

            Assert.Equal(
                Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder
                    .Entity<Nob>()
                    .OneToMany(e => e.Hobs, e => e.Nob)
                    .ForeignKey(e => new { e.NobId1, e.NobId2 })
                    .Required(false)).Message);

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void Many_to_one_relationships_with_nullable_keys_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Nob>()
                .ManyToOne(e => e.Hob, e => e.Nobs)
                .ForeignKey(e => new { e.HobId1, e.HobId2 })
                .Required();

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void Many_to_one_relationships_with_non_nullable_keys_cannot_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();

            Assert.Equal(
                Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder
                    .Entity<Hob>()
                    .ManyToOne(e => e.Nob, e => e.Hobs)
                    .ForeignKey(e => new { e.NobId1, e.NobId2 })
                    .Required(false)).Message);

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_with_nullable_keys_can_be_made_required()
        {
            var modelBuilder = HobNobBuilder();

            modelBuilder
                .Entity<Hob>()
                .OneToOne(e => e.Nob, e => e.Hob)
                .ForeignKey<Nob>(e => new { e.HobId1, e.HobId2 })
                .Required();

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Nob));

            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.False(entityType.GetProperty("HobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        [Fact]
        public void One_to_one_relationships_with_non_nullable_keys_cannot_be_made_optional()
        {
            var modelBuilder = HobNobBuilder();

            Assert.Equal(
                Strings.CannotBeNullable("NobId1", "Hob", "Int32"),
                Assert.Throws<InvalidOperationException>(() => modelBuilder
                    .Entity<Nob>()
                    .OneToOne(e => e.Hob, e => e.Nob)
                    .ForeignKey<Hob>(e => new { e.NobId1, e.NobId2 })
                    .Required(false)).Message);

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Hob));

            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.False(entityType.GetProperty("NobId1").IsNullable);
            Assert.True(entityType.ForeignKeys.Single().IsRequired);
        }

        private class Hob
        {
            public string Id1 { get; set; }
            public string Id2 { get; set; }

            public int NobId1 { get; set; }
            public int NobId2 { get; set; }

            public Nob Nob { get; set; }
            public ICollection<Nob> Nobs { get; set; }
        }

        private class Nob
        {
            public int Id1 { get; set; }
            public int Id2 { get; set; }

            public string HobId1 { get; set; }
            public string HobId2 { get; set; }

            public Hob Hob { get; set; }
            public ICollection<Hob> Hobs { get; set; }
        }

        private ModelBuilder HobNobBuilder()
        {
            var builder = new ModelBuilder();

            builder.Entity<Hob>().Key(e => new { e.Id1, e.Id2 });
            builder.Entity<Nob>().Key(e => new { e.Id1, e.Id2 });

            return builder;
        }

        [Fact]
        public void Generic_OneToMany_is_preserved_when_chaining_from_Annotation()
        {
            var modelBuilder = new ModelBuilder();

            AssertIsGenericOneToMany(modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .Annotation("X", "Y"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.Equal("Y", entityType.ForeignKeys.Single()["X"]);
        }

        [Fact]
        public void Generic_OneToMany_is_preserved_when_chaining_from_ForeignKey()
        {
            var modelBuilder = new ModelBuilder();

            AssertIsGenericOneToMany(modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ForeignKey("AnotherCustomerId"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.Equal("AnotherCustomerId", entityType.ForeignKeys.Single().Properties.Single().Name);
        }

        [Fact]
        public void Generic_OneToMany_is_preserved_when_chaining_from_ReferencedKey()
        {
            var modelBuilder = new ModelBuilder();

            AssertIsGenericOneToMany(modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .ReferencedKey("AlternateKey"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.Equal("AlternateKey", entityType.ForeignKeys.Single().ReferencedProperties.Single().Name);
        }

        [Fact]
        public void Generic_OneToMany_is_preserved_when_chaining_from_Required()
        {
            var modelBuilder = new ModelBuilder();

            AssertIsGenericOneToMany(modelBuilder
                .Entity<Customer>()
                .OneToMany(e => e.Orders, e => e.Customer)
                .Required(false));

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        private static void AssertIsGenericOneToMany(ModelBuilder.EntityBuilder<Customer>.OneToManyBuilder<Order> _)
        {
        }

        [Fact]
        public void Generic_ManyToOne_is_preserved_when_chaining_from_Annotation()
        {
            var modelBuilder = new ModelBuilder();

            AssertIsGenericManyToOne(modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .Annotation("X", "Y"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.Equal("Y", entityType.ForeignKeys.Single()["X"]);
        }

        [Fact]
        public void Generic_ManyToOne_is_preserved_when_chaining_from_ForeignKey()
        {
            var modelBuilder = new ModelBuilder();

            AssertIsGenericManyToOne(modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ForeignKey("AnotherCustomerId"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.Equal("AnotherCustomerId", entityType.ForeignKeys.Single().Properties.Single().Name);
        }

        [Fact]
        public void Generic_ManyToOne_is_preserved_when_chaining_from_ReferencedKey()
        {
            var modelBuilder = new ModelBuilder();

            AssertIsGenericManyToOne(modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .ReferencedKey("AlternateKey"));

            var entityType = modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.Equal("AlternateKey", entityType.ForeignKeys.Single().ReferencedProperties.Single().Name);
        }

        [Fact]
        public void Generic_ManyToOne_is_preserved_when_chaining_from_Required()
        {
            var modelBuilder = new ModelBuilder();

            AssertIsGenericManyToOne(modelBuilder
                .Entity<Order>()
                .ManyToOne(e => e.Customer, e => e.Orders)
                .Required(false));

            var entityType = (IEntityType)modelBuilder.Model.GetEntityType(typeof(Order));
            Assert.False(entityType.ForeignKeys.Single().IsRequired);
        }

        private static void AssertIsGenericManyToOne(ModelBuilder.EntityBuilder<Order>.ManyToOneBuilder<Customer> _)
        {
        }

        [Fact]
        public void OnEntityTypeAdded_calls_apply_on_conventions()
        {
            var model = new Model();
            var builder = new ModelBuilder(model);
            builder.EntityTypeConventions.Clear();
            var convention = new Mock<IEntityTypeConvention>();
            builder.EntityTypeConventions.Add(convention.Object);

            builder.Entity<Order>();

            convention.Verify(c => c.Apply(It.Is<InternalEntityBuilder>(t => t.Metadata.Type == typeof(Order))));
        }
    }
}
