﻿using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Interfaces
{
    public interface IChatMessageService : IGenericService<ChatMessage>
    {
        Task<Result<ChatMessage>> CreateChatMessage(AppUser appUser, Assignment assignment, string text);
    }
}
