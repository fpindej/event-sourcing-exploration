<script lang="ts">
	import * as Card from '$lib/components/ui/card';
	import { Button } from '$lib/components/ui/button';
	import { Input } from '$lib/components/ui/input';
	import {
		Plus,
		ArrowDownCircle,
		ArrowUpCircle,
		User,
		XCircle,
		Clock,
		Play,
		RotateCcw
	} from '@lucide/svelte';

	// Types for event sourcing
	interface BankAccount {
		id: string;
		accountHolder: string;
		balance: number;
		currency: string;
		isClosed: boolean;
		version: number;
	}

	interface EventData {
		eventId: string;
		eventType: string;
		eventData: Record<string, unknown>;
		version: number;
		occurredAt: string;
	}

	interface StateSnapshot {
		version: number;
		accountHolder: string;
		balance: number;
		currency: string;
		isClosed: boolean;
	}

	interface Timeline {
		account: BankAccount;
		events: EventData[];
		stateHistory: StateSnapshot[];
	}

	// State
	let accounts = $state<BankAccount[]>([]);
	let selectedAccountId = $state<string | null>(null);
	let timeline = $state<Timeline | null>(null);
	let selectedVersion = $state<number | null>(null);
	let isLoading = $state(false);
	let error = $state<string | null>(null);

	// Form state
	let newAccountHolder = $state('');
	let newAccountBalance = $state(1000);
	let depositAmount = $state(100);
	let depositDescription = $state('');
	let withdrawAmount = $state(50);
	let withdrawDescription = $state('');
	let newHolderName = $state('');
	let closeReason = $state('');

	const API_BASE = '/api/eventsourcing';

	// API calls
	async function fetchAccounts() {
		isLoading = true;
		error = null;
		try {
			const res = await fetch(`${API_BASE}/accounts`);
			if (!res.ok) throw new Error('Failed to fetch accounts');
			accounts = await res.json();
		} catch (e) {
			error = e instanceof Error ? e.message : 'Unknown error';
		} finally {
			isLoading = false;
		}
	}

	async function createAccount() {
		if (!newAccountHolder.trim()) return;
		isLoading = true;
		error = null;
		try {
			const res = await fetch(`${API_BASE}/accounts`, {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({
					accountHolder: newAccountHolder,
					initialBalance: newAccountBalance,
					currency: 'USD'
				})
			});
			if (!res.ok) {
				const text = await res.text();
				throw new Error(text || 'Failed to create account');
			}
			const account = await res.json();
			accounts = [...accounts, account];
			newAccountHolder = '';
			newAccountBalance = 1000;
			selectAccount(account.id);
		} catch (e) {
			error = e instanceof Error ? e.message : 'Unknown error';
		} finally {
			isLoading = false;
		}
	}

	async function selectAccount(accountId: string) {
		selectedAccountId = accountId;
		selectedVersion = null;
		await fetchTimeline(accountId);
	}

	async function fetchTimeline(accountId: string) {
		isLoading = true;
		error = null;
		try {
			const res = await fetch(`${API_BASE}/accounts/${accountId}/timeline`);
			if (!res.ok) throw new Error('Failed to fetch timeline');
			timeline = await res.json();
			selectedVersion = timeline?.account.version ?? null;
		} catch (e) {
			error = e instanceof Error ? e.message : 'Unknown error';
		} finally {
			isLoading = false;
		}
	}

	async function deposit() {
		if (!selectedAccountId || depositAmount <= 0) return;
		isLoading = true;
		error = null;
		try {
			const res = await fetch(`${API_BASE}/accounts/${selectedAccountId}/deposit`, {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({
					amount: depositAmount,
					description: depositDescription || 'Deposit'
				})
			});
			if (!res.ok) {
				const text = await res.text();
				throw new Error(text || 'Failed to deposit');
			}
			await fetchTimeline(selectedAccountId);
			await fetchAccounts();
			depositDescription = '';
		} catch (e) {
			error = e instanceof Error ? e.message : 'Unknown error';
		} finally {
			isLoading = false;
		}
	}

	async function withdraw() {
		if (!selectedAccountId || withdrawAmount <= 0) return;
		isLoading = true;
		error = null;
		try {
			const res = await fetch(`${API_BASE}/accounts/${selectedAccountId}/withdraw`, {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({
					amount: withdrawAmount,
					description: withdrawDescription || 'Withdrawal'
				})
			});
			if (!res.ok) {
				const text = await res.text();
				throw new Error(text || 'Failed to withdraw');
			}
			await fetchTimeline(selectedAccountId);
			await fetchAccounts();
			withdrawDescription = '';
		} catch (e) {
			error = e instanceof Error ? e.message : 'Unknown error';
		} finally {
			isLoading = false;
		}
	}

	async function changeHolder() {
		if (!selectedAccountId || !newHolderName.trim()) return;
		isLoading = true;
		error = null;
		try {
			const res = await fetch(`${API_BASE}/accounts/${selectedAccountId}/holder`, {
				method: 'PATCH',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ newName: newHolderName })
			});
			if (!res.ok) {
				const text = await res.text();
				throw new Error(text || 'Failed to change holder');
			}
			await fetchTimeline(selectedAccountId);
			await fetchAccounts();
			newHolderName = '';
		} catch (e) {
			error = e instanceof Error ? e.message : 'Unknown error';
		} finally {
			isLoading = false;
		}
	}

	async function closeAccount() {
		if (!selectedAccountId || !closeReason.trim()) return;
		isLoading = true;
		error = null;
		try {
			const res = await fetch(`${API_BASE}/accounts/${selectedAccountId}/close`, {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ reason: closeReason })
			});
			if (!res.ok) {
				const text = await res.text();
				throw new Error(text || 'Failed to close account');
			}
			await fetchTimeline(selectedAccountId);
			await fetchAccounts();
			closeReason = '';
		} catch (e) {
			error = e instanceof Error ? e.message : 'Unknown error';
		} finally {
			isLoading = false;
		}
	}

	function getEventIcon(eventType: string) {
		switch (eventType) {
			case 'AccountCreated':
				return Plus;
			case 'MoneyDeposited':
				return ArrowDownCircle;
			case 'MoneyWithdrawn':
				return ArrowUpCircle;
			case 'AccountHolderChanged':
				return User;
			case 'AccountClosed':
				return XCircle;
			default:
				return Clock;
		}
	}

	function getEventColor(eventType: string) {
		switch (eventType) {
			case 'AccountCreated':
				return 'bg-blue-500';
			case 'MoneyDeposited':
				return 'bg-green-500';
			case 'MoneyWithdrawn':
				return 'bg-orange-500';
			case 'AccountHolderChanged':
				return 'bg-purple-500';
			case 'AccountClosed':
				return 'bg-red-500';
			default:
				return 'bg-gray-500';
		}
	}

	function formatCurrency(amount: number, currency: string) {
		return new Intl.NumberFormat('en-US', {
			style: 'currency',
			currency
		}).format(amount);
	}

	function formatDate(dateString: string) {
		return new Date(dateString).toLocaleString();
	}

	function getStateAtVersion(version: number): StateSnapshot | null {
		return timeline?.stateHistory.find((s) => s.version === version) ?? null;
	}

	// Initialize
	$effect(() => {
		fetchAccounts();
	});

	let currentState = $derived(selectedVersion ? getStateAtVersion(selectedVersion) : null);
	let selectedAccount = $derived(accounts.find((a) => a.id === selectedAccountId) ?? null);
</script>

<svelte:head>
	<title>Event Sourcing Explorer</title>
	<meta
		name="description"
		content="Explore event sourcing concepts with a bank account example"
	/>
</svelte:head>

<div class="space-y-6">
	<div>
		<h1 class="text-3xl font-bold tracking-tight">Event Sourcing Explorer</h1>
		<p class="text-muted-foreground">
			Explore event sourcing concepts with a bank account example. Create accounts, perform
			transactions, and see how state is reconstructed from events.
		</p>
	</div>

	{#if error}
		<div class="rounded-lg border border-red-200 bg-red-50 p-4 text-red-700 dark:border-red-800 dark:bg-red-950 dark:text-red-300">
			{error}
			<Button variant="ghost" size="sm" onclick={() => (error = null)} class="ml-2">
				Dismiss
			</Button>
		</div>
	{/if}

	<div class="grid gap-6 lg:grid-cols-3">
		<!-- Accounts List -->
		<Card.Root>
			<Card.Header>
				<Card.Title class="flex items-center gap-2">
					<User class="h-5 w-5" />
					Bank Accounts
				</Card.Title>
				<Card.Description>Select an account to view its event history</Card.Description>
			</Card.Header>
			<Card.Content class="space-y-4">
				<!-- Create Account Form -->
				<div class="space-y-2 rounded-lg border p-3">
					<Input
						bind:value={newAccountHolder}
						placeholder="Account holder name"
						disabled={isLoading}
					/>
					<Input
						type="number"
						bind:value={newAccountBalance}
						placeholder="Initial balance"
						min="0"
						disabled={isLoading}
					/>
					<Button
						onclick={createAccount}
						disabled={isLoading || !newAccountHolder.trim()}
						class="w-full"
					>
						<Plus class="mr-2 h-4 w-4" />
						Create Account
					</Button>
				</div>

				<!-- Account List -->
				<div class="space-y-2">
					{#each accounts as account (account.id)}
						<button
							class="w-full rounded-lg border p-3 text-left transition-colors hover:bg-muted {selectedAccountId ===
							account.id
								? 'border-primary bg-muted'
								: ''}"
							onclick={() => selectAccount(account.id)}
						>
							<div class="flex items-center justify-between">
								<span class="font-medium">{account.accountHolder}</span>
								{#if account.isClosed}
									<span class="rounded bg-red-100 px-2 py-0.5 text-xs text-red-700 dark:bg-red-900 dark:text-red-300">
										Closed
									</span>
								{/if}
							</div>
							<div class="text-sm text-muted-foreground">
								{formatCurrency(account.balance, account.currency)}
							</div>
							<div class="text-xs text-muted-foreground">Version: {account.version}</div>
						</button>
					{:else}
						<p class="text-center text-muted-foreground">No accounts yet. Create one above!</p>
					{/each}
				</div>
			</Card.Content>
		</Card.Root>

		<!-- Actions & Current State -->
		<Card.Root>
			<Card.Header>
				<Card.Title class="flex items-center gap-2">
					<Play class="h-5 w-5" />
					Actions
				</Card.Title>
				<Card.Description>Perform operations on the selected account</Card.Description>
			</Card.Header>
			<Card.Content class="space-y-4">
				{#if selectedAccount && !selectedAccount.isClosed}
					<!-- Deposit -->
					<div class="space-y-2 rounded-lg border p-3">
						<h4 class="font-medium text-green-600 dark:text-green-400">Deposit</h4>
						<Input
							type="number"
							bind:value={depositAmount}
							placeholder="Amount"
							min="0.01"
							step="0.01"
						/>
						<Input bind:value={depositDescription} placeholder="Description (optional)" />
						<Button
							onclick={deposit}
							disabled={isLoading || depositAmount <= 0}
							variant="outline"
							class="w-full"
						>
							<ArrowDownCircle class="mr-2 h-4 w-4 text-green-500" />
							Deposit
						</Button>
					</div>

					<!-- Withdraw -->
					<div class="space-y-2 rounded-lg border p-3">
						<h4 class="font-medium text-orange-600 dark:text-orange-400">Withdraw</h4>
						<Input
							type="number"
							bind:value={withdrawAmount}
							placeholder="Amount"
							min="0.01"
							step="0.01"
						/>
						<Input bind:value={withdrawDescription} placeholder="Description (optional)" />
						<Button
							onclick={withdraw}
							disabled={isLoading || withdrawAmount <= 0}
							variant="outline"
							class="w-full"
						>
							<ArrowUpCircle class="mr-2 h-4 w-4 text-orange-500" />
							Withdraw
						</Button>
					</div>

					<!-- Change Holder -->
					<div class="space-y-2 rounded-lg border p-3">
						<h4 class="font-medium text-purple-600 dark:text-purple-400">Change Holder</h4>
						<Input bind:value={newHolderName} placeholder="New holder name" />
						<Button
							onclick={changeHolder}
							disabled={isLoading || !newHolderName.trim()}
							variant="outline"
							class="w-full"
						>
							<User class="mr-2 h-4 w-4 text-purple-500" />
							Change Name
						</Button>
					</div>

					<!-- Close Account -->
					<div class="space-y-2 rounded-lg border p-3">
						<h4 class="font-medium text-red-600 dark:text-red-400">Close Account</h4>
						<Input bind:value={closeReason} placeholder="Reason for closing" />
						<Button
							onclick={closeAccount}
							disabled={isLoading || !closeReason.trim()}
							variant="destructive"
							class="w-full"
						>
							<XCircle class="mr-2 h-4 w-4" />
							Close Account
						</Button>
					</div>
				{:else if selectedAccount?.isClosed}
					<div
						class="rounded-lg border border-red-200 bg-red-50 p-4 text-center text-red-700 dark:border-red-800 dark:bg-red-950 dark:text-red-300"
					>
						This account is closed. No further actions are allowed.
					</div>
				{:else}
					<div class="rounded-lg border p-8 text-center text-muted-foreground">
						Select an account to perform actions
					</div>
				{/if}
			</Card.Content>
		</Card.Root>

		<!-- Current/Historical State -->
		<Card.Root>
			<Card.Header>
				<Card.Title class="flex items-center gap-2">
					<RotateCcw class="h-5 w-5" />
					State Viewer
				</Card.Title>
				<Card.Description>View the account state at any point in time</Card.Description>
			</Card.Header>
			<Card.Content>
				{#if timeline && currentState}
					<div class="space-y-4">
						<!-- Version Slider -->
						<div class="space-y-2">
							<label class="text-sm font-medium">
								Time Travel: Version {selectedVersion} of {timeline.account.version}
							</label>
							<input
								type="range"
								min="1"
								max={timeline.account.version}
								bind:value={selectedVersion}
								class="w-full"
							/>
							<div class="flex justify-between text-xs text-muted-foreground">
								<span>Created</span>
								<span>Current</span>
							</div>
						</div>

						<!-- State Display -->
						<div class="space-y-3 rounded-lg border p-4">
							<div class="flex items-center justify-between">
								<span class="text-muted-foreground">Account Holder</span>
								<span class="font-medium">{currentState.accountHolder}</span>
							</div>
							<div class="flex items-center justify-between">
								<span class="text-muted-foreground">Balance</span>
								<span class="font-medium text-lg">
									{formatCurrency(currentState.balance, currentState.currency)}
								</span>
							</div>
							<div class="flex items-center justify-between">
								<span class="text-muted-foreground">Currency</span>
								<span class="font-medium">{currentState.currency}</span>
							</div>
							<div class="flex items-center justify-between">
								<span class="text-muted-foreground">Status</span>
								<span
									class="rounded px-2 py-0.5 text-sm {currentState.isClosed
										? 'bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300'
										: 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300'}"
								>
									{currentState.isClosed ? 'Closed' : 'Active'}
								</span>
							</div>
						</div>

						{#if selectedVersion !== timeline.account.version}
							<div class="rounded-lg border border-yellow-200 bg-yellow-50 p-3 text-sm text-yellow-700 dark:border-yellow-800 dark:bg-yellow-950 dark:text-yellow-300">
								⏰ You're viewing a historical state. The current version is {timeline.account
									.version}.
							</div>
						{/if}
					</div>
				{:else}
					<div class="rounded-lg border p-8 text-center text-muted-foreground">
						Select an account to view its state
					</div>
				{/if}
			</Card.Content>
		</Card.Root>
	</div>

	<!-- Event Timeline -->
	{#if timeline && timeline.events.length > 0}
		<Card.Root>
			<Card.Header>
				<Card.Title class="flex items-center gap-2">
					<Clock class="h-5 w-5" />
					Event Stream
				</Card.Title>
				<Card.Description>
					Complete history of all events for this account. Events are immutable - they can never
					be changed or deleted.
				</Card.Description>
			</Card.Header>
			<Card.Content>
				<div class="relative">
					<!-- Timeline line -->
					<div class="absolute left-4 top-0 h-full w-0.5 bg-border"></div>

					<div class="space-y-4">
						{#each timeline.events as event, index (event.eventId)}
							{@const EventIcon = getEventIcon(event.eventType)}
							{@const isSelected = selectedVersion === event.version}
							<div
								class="relative flex gap-4 {isSelected ? 'opacity-100' : selectedVersion && selectedVersion < event.version ? 'opacity-40' : 'opacity-100'}"
							>
								<!-- Icon -->
								<div
									class="relative z-10 flex h-8 w-8 shrink-0 items-center justify-center rounded-full {getEventColor(
										event.eventType
									)} text-white"
								>
									<EventIcon class="h-4 w-4" />
								</div>

								<!-- Content -->
								<div
									class="flex-1 rounded-lg border p-4 transition-colors {isSelected
										? 'border-primary bg-muted'
										: ''}"
								>
									<div class="flex items-start justify-between">
										<div>
											<h4 class="font-medium">{event.eventType}</h4>
											<p class="text-sm text-muted-foreground">
												Version {event.version} • {formatDate(event.occurredAt)}
											</p>
										</div>
										<Button
											variant="ghost"
											size="sm"
											onclick={() => (selectedVersion = event.version)}
										>
											View State
										</Button>
									</div>

									<!-- Event Data -->
									<div class="mt-2 rounded bg-muted/50 p-2">
										<pre class="text-xs overflow-x-auto">{JSON.stringify(
												event.eventData,
												null,
												2
											)}</pre>
									</div>
								</div>
							</div>
						{/each}
					</div>
				</div>
			</Card.Content>
		</Card.Root>
	{/if}

	<!-- Explanation Card -->
	<Card.Root>
		<Card.Header>
			<Card.Title>📚 What is Event Sourcing?</Card.Title>
		</Card.Header>
		<Card.Content class="prose prose-sm dark:prose-invert max-w-none">
			<p>
				<strong>Event Sourcing</strong> is a pattern where the state of an application is determined
				by a sequence of events rather than storing just the current state.
			</p>
			<h4>Key Concepts Demonstrated:</h4>
			<ul>
				<li>
					<strong>Events are immutable</strong> - Once an event is stored, it never changes. This
					provides a complete audit trail.
				</li>
				<li>
					<strong>State is derived</strong> - The current state is calculated by replaying all
					events in order.
				</li>
				<li>
					<strong>Time travel</strong> - You can reconstruct the state at any point in time by
					replaying events up to that version.
				</li>
				<li>
					<strong>Aggregates</strong> - Events are grouped by aggregate (in this case, a bank
					account). Each aggregate has its own event stream.
				</li>
			</ul>
			<h4>Benefits:</h4>
			<ul>
				<li>Complete audit trail of all changes</li>
				<li>Ability to debug issues by replaying events</li>
				<li>Support for temporal queries (what was the state at time X?)</li>
				<li>Event-driven integrations with other systems</li>
			</ul>
		</Card.Content>
	</Card.Root>
</div>
