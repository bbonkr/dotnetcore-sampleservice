using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Sample.MicroServices.Entities;

namespace Sample.MicroServices.Authorization.Data.EntityTypeConfiguration
{
    internal class RefreshTokenEntityTypeConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(x => x.Id);

            builder
                .Property(x => x.Id)
                .HasMaxLength(StringLengths.Identifier)
                .IsRequired()
                .HasComment("식별자")
                ;
            builder
                .Property(x => x.UserId)
                .HasMaxLength(StringLengths.Identifier)
                .IsRequired()
                .HasComment("사용자 식별자")
                ;
            builder
                .Property(x => x.Token)
                .HasMaxLength(StringLengths.Long)
                .IsRequired()
                .HasComment("리프레시 토큰")
                ;
            builder
                .Property(x => x.Expires)
                .IsRequired()
                .HasComment("만료시각")
                ;
            builder
                .Property(x => x.Created)
                .IsRequired()
                .HasComment("작성시각")
                ;
            builder
                .Property(x => x.CreatedByIp)
                .HasMaxLength(StringLengths.IpAddress)
                .IsRequired()
                .HasComment("작성 요청 아이피 주소")
                ;
            builder
                .Property(x => x.Revoked)
                .IsRequired(false)
                .HasComment("취소시각")
                ;
            builder
                .Property(x => x.RevokedByIp)
                .HasMaxLength(StringLengths.IpAddress)
                .IsRequired(false)
                .HasComment("취소 요청 아이피 주소")
                ;
            builder
                .Property(x => x.ReplacedByToken)
                .HasMaxLength(StringLengths.Long)
                .IsRequired(false)
                .HasComment("취소 요청 토큰")
                ;

            builder
                .HasOne(t => t.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(t => t.UserId)
                ;
            
        }
    }
}
