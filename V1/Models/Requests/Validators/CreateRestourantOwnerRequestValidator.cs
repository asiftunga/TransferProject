using FluentValidation;

namespace MiniApp1Api.V1.Models.Requests.Validators;

public class CreateRestourantOwnerRequestValidator : AbstractValidator<CreateRestourantAndOwnerRequest>
{
    public CreateRestourantOwnerRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .Length(3,32)
            .Matches("^[a-zA-Z ]*$").WithMessage("First name can only contain letters and spaces.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .Length(3,32)
            .Matches("^[a-zA-Z ]*$").WithMessage("LastName name can only contain letters and spaces.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .Length(3, 320)
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Parola boş bırakılamaz.")
            .MinimumLength(10).WithMessage("Parola en az 10 karakter olmalıdır.")
            .MaximumLength(32).WithMessage("Parola en fazla 32 karakter olabilir.")
            .Matches(@"[0-9]").WithMessage("Parola en az bir sayı içermelidir.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası boş bırakılamaz.")
            .Length(8, 12).WithMessage("Telefon numarası minimum 8 ve maksimum 12 karakter olmalıdır.")
            .Matches(@"^\+?\d+$").WithMessage("Telefon numarası sadece sayılardan oluşmalıdır.");

        RuleFor(x => x.RestourantPhoneNumber)
            .Length(8, 12).WithMessage("Telefon numarası minimum 8 ve maksimum 12 karakter olmalıdır.")
            .Matches(@"^\+?\d+$").WithMessage("Telefon numarası sadece sayılardan oluşmalıdır.").When(x => x
            .RestourantPhoneNumber != null);

        RuleFor(x => x.RestaurantName)
            .NotEmpty().WithMessage("Restoran adı boş bırakılamaz.")
            .MinimumLength(3).WithMessage("Restoran ismi en az 3 karakter içermelidir.")
            .MaximumLength(64).WithMessage("Restoran ismi en fazla 64 karakter içermelidir.")
            .Matches(@"^[a-zA-Z\s]+$").WithMessage("Restoran ismi sadece harf ve boşluk içermelidir.");

        RuleFor(x => x.PassportOrTaxNumber)
            .NotEmpty().WithMessage("Pasaport numarası boş bırakılamaz.")
            .Length(10).WithMessage("Pasaport numarası 10 karakter olmak zorundadır.")
            .Matches(@"^\d+$").WithMessage("Pasaport numarası sadece sayı içerebilir.");


    }
}