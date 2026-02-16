# LlmPromptToolkit Web App Development Roadmap

## Phase 1: Basic Web App with Single Request
Convert console app to ASP.NET Core MVC web app with simple prompt submission and response display.

### Tasks
- [x] Create new ASP.NET Core MVC project (or convert existing console app directory)
- [x] Setup project structure (Controllers, Views, Models, Services)
- [x] Configure dependency injection for OllamaService
- [x] Create HomeController with Index action
- [x] Create Index.cshtml view with prompt textarea and send button
- [x] Implement form POST to send request to Ollama
- [x] Display response text in response area
- [x] Display timing metrics (response time in ms, token count)
- [x] Add basic Bootstrap styling for layout
- [x] Test: Submit prompt → receive response → verify timing displays

**Integration Test Success Criteria:**
- ✓ Form submits without errors
- ✓ Response displays within 5 seconds
- ✓ Timing metrics show correct ms value
- ✓ Response text renders without formatting issues

---

## Phase 2: Prompt File Management
Implement CRUD for prompt files stored in filesystem.

### Tasks
- [x] Create PromptsController with Index, Create, Edit, Delete actions
- [x] Create Prompt model class with properties (id, name, content, createdAt, updatedAt)
- [x] Implement PromptService.GetAllPromptsAsync() to read from Data/Prompts/
- [x] Implement PromptService.SavePromptAsync() to write prompt JSON
- [x] Create Prompts/Index.cshtml to list all prompts with edit/delete buttons
- [x] Create Prompts/Form.cshtml for create/edit with textarea, name input, submit button
- [x] Add modal or page for delete confirmation
- [x] Update HomeController to have prompt dropdown selector
- [x] Add "Use Prompt" button that loads selected prompt into builder
- [x] Test: Create prompt → display in list → select in builder → content loads
- [x] Test: Edit prompt → verify changes saved → verify previous version replaced
- [x] Test: Delete prompt → verify removed from list and filesystem

**Integration Test Success Criteria:**
- ✓ Create new prompt creates JSON file in Data/Prompts/
- ✓ Prompt appears in list immediately after creation
- ✓ Select prompt from dropdown loads content into request builder
- ✓ Edit updates JSON file correctly
- ✓ Delete removes file from filesystem and list

---

## Phase 3: JSON Validation Service
Build validation service and display results after response.

### Tasks
- [ ] Create JsonValidationService class with ValidateJsonAsync() method
- [ ] Implement JSON parse validation (catches malformed JSON)
- [ ] Add RequiredFields property to Prompt model
- [ ] Implement RequiredFieldsValidator to check field population
- [ ] Create ValidationResult model (isValid, errors list, requiredFieldsStatus)
- [ ] Create ValidationResultsPanel partial view component
- [ ] Integrate validation into request response flow (validate after getting response)
- [ ] Display validation success alert with required fields checklist
- [ ] Display validation failure alert with error list and failed fields
- [ ] Add toggle to show/hide full response when validation fails
- [ ] Test: Valid JSON response → validation passes → checklist shows all fields
- [ ] Test: Invalid JSON → validation fails → parse error displays
- [ ] Test: Missing required field → validation fails → error shows which field missing

**Integration Test Success Criteria:**
- ✓ Valid responses show success badge with all fields checked
- ✓ Invalid JSON shows parse error message
- ✓ Missing required fields listed with field names
- ✓ Validation errors prevent accidental acceptance of bad responses

---

## Phase 4: Response Storage & Chain History
Save responses to filesystem and display chain history.

### Tasks
- [ ] Create Response model (id, promptText, responseText, responseContext, timingMs, validation, createdAt)
- [ ] Create Chain model (id, name, responseIds list, createdAt)
- [ ] Create ChainService.CreateChainAsync() to create chain JSON
- [ ] Create ChainService.AddResponseAsync() to append response to chain
- [ ] Implement SaveResponseAsync() to write response JSON to Data/Responses/
- [ ] Create Chains/Index.cshtml to list all chains with response counts
- [ ] Create Chains/View.cshtml to show chain with all responses as list
- [ ] Add "Save to Chain" button on response display with chain selector
- [ ] Implement "Create New Chain" option in chain selector
- [ ] Display response details (prompt, response, timing, validation) in expandable list items
- [ ] Test: Send request → click "Save to Chain" → select/create chain → response saved
- [ ] Test: View chain → all responses display with metadata
- [ ] Test: Multiple responses in chain → ordering preserved

**Integration Test Success Criteria:**
- ✓ Response JSON written to Data/Responses/ with correct structure
- ✓ Chain JSON updated with new responseId
- ✓ Chains/View shows all responses in order
- ✓ Response metadata (timing, validation) displays correctly

---

## Phase 5: Modelfile Initialization
Mark prompts as modelfiles and initialize chains with cached context.

### Tasks
- [ ] Add isModelFile boolean property to Prompt model
- [ ] Add UI checkbox to mark prompt as modelfile when editing
- [ ] Create ChainService.InitializeChainWithModelfileAsync() method
- [ ] Implement modelfile initialization: send prompt → cache responseContext as modelFileContextTokens
- [ ] Update Chain model with modelFilePromptId, modelFileContextTokens, modelFileInitializedAt
- [ ] Create "Initialize Chain with Modelfile" workflow in Chains/Create view
- [ ] Implement automatic context prepending in ChainService.SendRequestWithModelfileContextAsync()
- [ ] Create ModelfileStatusPanel partial to show cached context info
- [ ] Display modelfile name, token count, initialization time in request builder
- [ ] Add "Re-initialize Modelfile" button to refresh context
- [ ] Test: Mark prompt as modelfile → initialize chain → verify context cached
- [ ] Test: Send request with modelfile context → verify responseContext prepended
- [ ] Test: Send second request → confirm same modelfile context used (no re-initialization)

**Integration Test Success Criteria:**
- ✓ Modelfile marked in prompt metadata
- ✓ Chain initialization sends modelfile and caches context
- ✓ Request1 uses modelfile context
- ✓ Request2 uses same cached context (not new initialization)
- ✓ Context token count matches expected value

---

## Phase 6: Branching from Response Context
Allow branching requests from any response in chain.

### Tasks
- [ ] Update Response model: add parentResponseId (null if from modelfile), parentContextTokens
- [ ] Update Chain model: change responseIds to full response tree structure
- [ ] Create ChainService.GetAvailableContextsAsync() - returns modelfile + all response contexts
- [ ] Create ContextInfo model (sourceId, displayLabel, tokenCount, createdAt, sourcePrompt)
- [ ] Create context selector UI in request builder (radio buttons + dropdown)
- [ ] Update request form to show available contexts with token counts and creation times
- [ ] Implement ChainService.SendRequestWithContextAsync(chainId, promptText, contextSourceId)
- [ ] Store parentResponseId and parentContextTokens when request sent from non-modelfile context
- [ ] Test: Create request1 from modelfile context
- [ ] Test: Create request1a branching from request1's responseContext
- [ ] Test: Create request2 branching from request1a's responseContext
- [ ] Test: Verify parentResponseId and parentContextTokens stored correctly for each

**Integration Test Success Criteria:**
- ✓ GetAvailableContextsAsync returns modelfile + all responses
- ✓ Context selector shows all options with labels, tokens, timestamps
- ✓ Selecting response context and sending request uses that context
- ✓ parentResponseId correctly identifies parent for each response
- ✓ parentContextTokens matches actual context used

---

## Phase 7: Chain Visualization
Build tree view showing branching relationships and all response metadata.

### Tasks
- [ ] Create ChainService.GetChainTreeAsync() to build hierarchical response structure
- [ ] Design tree rendering HTML (nested divs with indentation or visual connectors)
- [ ] Create Chains/TreeView.cshtml partial component for tree visualization
- [ ] Add response node display: number, prompt summary, timing badge, validation status
- [ ] Add visual indicators for modelfile root and branch points
- [ ] Implement click-to-expand/collapse for tree nodes
- [ ] Add inline metadata display: response time (ms), token count, validation result
- [ ] Create response detail sidebar: click node → show full response, context tokens, prompt
- [ ] Add "Edit & Resend" button to retry request with modifications
- [ ] Add "Branch from here" button to quickly create child request
- [ ] Implement tree-to-JSON export for sharing/documentation
- [ ] Test: Create multi-level branched chain (modelfile → 2 requests → 2 branches from request1)
- [ ] Test: Verify tree renders with correct hierarchy and all metadata visible
- [ ] Test: Click node → sidebar shows correct response details
- [ ] Test: Branch from deep node → new response appears as child in tree

**Integration Test Success Criteria:**
- ✓ Tree renders with correct parent-child relationships
- ✓ All responses visible with correct ordering
- ✓ Timing badges show accurate metrics
- ✓ Validation status displays correctly for each response
- ✓ Sidebar shows full details when node clicked
- ✓ New branches appear in correct position in tree
- ✓ Expand/collapse works without losing state

---

## Notes
- Each phase builds on previous phases
- Integration tests should verify both UI and filesystem state
- Consider adding logging for debugging phase transitions
- Keep console app functional alongside web app during Phase 1-2
- Phases 5-7 benefit from comprehensive testing due to tree complexity

## Success Metrics (per phase)
| Phase | Manual Testing | Automated Testing | Review |
|-------|---|---|---|
| 1 | ✓ Form submit, response display | Basic happy path | Console app still works |
| 2 | ✓ CRUD operations | File I/O | Filesystem structure correct |
| 3 | ✓ Valid/invalid JSON | Validation logic | Error messages clear |
| 4 | ✓ Chain creation, history view | Response persistence | File structure matches model |
| 5 | ✓ Modelfile init, context reuse | Context prepending | Token counts accurate |
| 6 | ✓ Branch selection, parent tracking | Tree structure | Parent-child relationships correct |
| 7 | ✓ Tree rendering, navigation | No regressions | Performance acceptable with large trees |
