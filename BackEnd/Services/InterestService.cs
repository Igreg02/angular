using RubricaSemplice.Api.Data;
using RubricaSemplice.Api.Dtos;
using RubricaSemplice.Api.Models;

namespace RubricaSemplice.Api.Services;

public class InterestService // gestisce la logica delle operazioni CRUD effettuabili sugli interessi
{
    private readonly ApplicationDbContext _context; // context è il database di EntityFramework

    public InterestService(ApplicationDbContext context)
    {
        _context = context; // dependency injection per poter aver il database pubblico
    }

    public async Task<List<InterestDto>> GetAllByUserIdAsync(string userId) // metodo asincrono che torna tutti gli interessi rispetto all'userId.
    {
        List<InterestDto> result = new List<InterestDto>();

        // Leggiamo tutti gli interessi e poi filtriamo con un ciclo.
        // È meno efficiente del LINQ, ma è più facile da capire all'inizio.
        List<Interest> allInterests = _context.Interests.ToList();

        for (int i = 0; i < allInterests.Count; i++)
        {
            Interest currentInterest = allInterests[i];

            if (currentInterest.UserId == userId)
            {
                InterestDto dto = new InterestDto();
                dto.Id = currentInterest.Id;
                dto.Nome = currentInterest.Nome;

                result.Add(dto);
            }
        }

        return await Task.FromResult(result);
    }

    public async Task<InterestDto?> GetByIdAsync(int id, string userId) // metodo asincrono per ottenere un interesse per UserId inserito e Id dell'interesse
    {
        Interest? interest = await _context.Interests.FindAsync(id);

        if (interest == null)
        {
            return null;
        }

        if (interest.UserId != userId)
        {
            return null;
        }

        InterestDto dto = new InterestDto();
        dto.Id = interest.Id;
        dto.Nome = interest.Nome;

        return dto;
    }

    public async Task<InterestDto?> CreateAsync(InterestCreateDto dto, string userId) // metodo asincrono per la creazione di un interesse
    {
        // Evitiamo interessi duplicati per lo stesso utente
        List<Interest> allInterests = _context.Interests.ToList();

        for (int i = 0; i < allInterests.Count; i++)
        {
            Interest currentInterest = allInterests[i];

            if (currentInterest.UserId == userId)
            {
                bool sameName = string.Equals(currentInterest.Nome, dto.Nome, StringComparison.OrdinalIgnoreCase);

                // se il booleano è true allora non si crea un nuovo interesse, essendo già presente.
                if (sameName)
                {
                    return null;
                }
            }
        }

        // creazione del nuovo interesse.
        Interest interest = new Interest();
        interest.Nome = dto.Nome;
        interest.UserId = userId;

        _context.Interests.Add(interest);
        await _context.SaveChangesAsync(); // salva le modifiche apportate al database.

        InterestDto result = new InterestDto();
        result.Id = interest.Id;
        result.Nome = interest.Nome;

        return result;
    }

    public async Task<InterestDto?> UpdateAsync(int id, InterestCreateDto dto, string userId) // metodo per la modifica di un interesse.
    {
        Interest? interest = await _context.Interests.FindAsync(id);

        if (interest == null)
        {
            return null;
        }

        if (interest.UserId != userId)
        {
            return null;
        }

        // Controlliamo che il nuovo nome non esista già in un altro interesse dello stesso utente
        List<Interest> allInterests = _context.Interests.ToList();

        for (int i = 0; i < allInterests.Count; i++)
        {
            Interest currentInterest = allInterests[i];

            if (currentInterest.UserId == userId && currentInterest.Id != id)
            {
                bool sameName = string.Equals(currentInterest.Nome, dto.Nome, StringComparison.OrdinalIgnoreCase);

                if (sameName)
                {
                    return null;
                }
            }
        }

        interest.Nome = dto.Nome;
        await _context.SaveChangesAsync(); // salva le modifiche apportate al database.

        InterestDto result = new InterestDto();
        result.Id = interest.Id;
        result.Nome = interest.Nome;

        return result;
    }

    public async Task<bool> DeleteAsync(int id, string userId) // metodo per la cancellazione di un interesse.
    {
        Interest? interest = await _context.Interests.FindAsync(id);

        if (interest == null)
        {
            return false;
        }

        if (interest.UserId != userId)
        {
            return false;
        }

        _context.Interests.Remove(interest);
        await _context.SaveChangesAsync();

        return true;
    }
}