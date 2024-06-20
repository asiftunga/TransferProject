using FluentValidation;

namespace TransferProject.V1.Models.Requests.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
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


    }
}