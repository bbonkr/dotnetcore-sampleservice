using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SampleService.Entities;

namespace SampleService.Data.EntityTypeConfiguration.EntityTypeConfiguration
{
    public class AuthorizationLogEntityTypeConfiguration : IEntityTypeConfiguration<AuthorizationLog>
    {
        public void Configure(EntityTypeBuilder<AuthorizationLog> builder)
        {
            builder.HasComment("인증로그");

            builder.HasKey(x => x.Id);
            
            builder
                .Property(x => x.Id)
                .IsRequired()
                .HasMaxLength(StringLengths.Identifier)
                .ValueGeneratedOnAdd()
                .HasComment("식별자")
                ;
            builder
                .Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(StringLengths.Name)
                .HasComment("사용자 계정이름")
                ;
            builder
                .Property(x => x.IsSuccess)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("성공여부")
                ;
            builder
                .Property(x => x.IpAddress)
                .HasMaxLength(StringLengths.IpAddress)
                .HasComment("아이피 주소")
                ;
            builder
                .Property(x => x.Hostname)
                .HasMaxLength(StringLengths.Name)
                .HasComment("기기명칭")
                ;
            builder
                .Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValue(DateTimeOffset.UtcNow)
                .HasComment("작성시각")
                ;

        }
    }
}
