using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SampleService.Entities;

namespace SampleService.Data.EntityTypeConfiguration
{
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .HasMaxLength(StringLengths.Identifier)
                .IsRequired()
                .ValueGeneratedOnAdd()
                .HasComment("식별자")
                ;
            builder
                .Property(x => x.FirstName)
                .HasMaxLength(StringLengths.Name)
                .IsRequired()
                .HasComment("성")
                ;
            builder
                .Property(x => x.LastName)
                .HasMaxLength(StringLengths.Name)
                .IsRequired()
                .HasComment("이름")
                ;
            builder
                .Property(x => x.UserName)
                .HasMaxLength(StringLengths.Name)
                .IsRequired()
                .HasComment("계정이름")
                ;
            builder
                .Property(x => x.Password)
                .HasMaxLength(StringLengths.Long)
                .HasComment("비밀번호")
                ;
            builder
                .Property(x => x.IsEnabled)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("사용여부")
                ;
            builder
                .Property(x => x.FailCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("인증 실패수")
                ;
            builder
                .Property(x => x.IsLocked)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("계정 잠금 여부")
                ;

        }
    }
}
