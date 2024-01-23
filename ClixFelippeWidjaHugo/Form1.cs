using Microsoft.VisualBasic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Eventing.Reader;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ClixFelippeWidjaHugo
{
    public partial class Form1 : Form
    {
        Database database = new Database();
        Processo processo = new Processo();
        Cliente cliente = new Cliente();
        Funcionario funcionario = new Funcionario();
        Categoria categoria = new Categoria();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //preenche cbxCategoria com dados da tabela Categorias.
            PreencherCbxCategoriaAddRegistos();
            PreencherCbxFuncionario();
            PreencherCbxCliente();
            PreencherCbxRemoverCategoria();
            CarregarAutoCompleteCliente();
            CarregarAutoCompleteFuncionario();

            // posi�ao no form
            panelAdministracao.Location = new Point(3, 40);
            panelRegistrarProcesso.Location = new Point(3, 40);
            panelVisualizarProcessos.Location = new Point(3, 40);

            // tira o foco da txtBox para usuario visualizar placeholder.
            lblRegisto.Select();

            btnVisualizarProcessos_Click(e, e);

        }

        /* Adiciona um novo processo a base de dados, de
         acordo com os dados preenchidos nos campos da �rea 'Registrar Processo' */
        private void btnEnviar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtAutoCliente.Text) || string.IsNullOrEmpty(txtAutoFuncionario.Text) || string.IsNullOrEmpty(txtDescricao.Text) || string.IsNullOrEmpty(txtTempoGasto.Text))
            {
                MessageBox.Show("Preencha todos os campos.");
            }
            else
            {
                string descricao = txtDescricao.Text;
                string data = dtpDataProcesso.Value.ToString("yyyy-MM-dd");
                string tempoGasto = txtTempoGasto.Text;
                string idFuncionario = txtAutoFuncionario.Text.Substring(0, 1);
                string idCliente = txtAutoCliente.Text.Substring(0, 1);
                string idCategoria = cbxCategoria.SelectedValue.ToString();

                try
                {
                    processo.AdicionarProcesso(descricao, data, tempoGasto, idFuncionario, idCliente, idCategoria);
                    MessageBox.Show("Registo adicionado com sucesso!");
                    btnListar_Click(e, e);
                }
                catch (Exception)
                {
                    MessageBox.Show("Erro. Registo n�o adicionado.");
                }
            }
        }

        /* Ao clicar no bot�o Listar, exibe todos os processos de acordo
         com a op��o de filtragem selecionada */
        private void btnListar_Click(object sender, EventArgs e)
        {
            DataTable dataTable = new DataTable();
            string idClienteFuncionario = "";

            if (cbxFiltro.SelectedIndex != -1)
            {
                idClienteFuncionario = cbxFiltro.Text.Substring(0, 1);
            }

            if (rbtNaoAgrupar.Checked == false && cbxFiltro.SelectedIndex == -1)
            {
                MessageBox.Show("Preencha o campo ID de acordo com o filtro selecionado.");
                rbtNaoAgrupar.Checked = true;
            }
            else
            {
                try
                {
                    dataTable = processo.ListarProcessos(idClienteFuncionario, rbtCliente.Checked, rbtFuncionario.Checked);
                    dgvProcessos.DataSource = dataTable;

                    if (rbtFuncionario.Checked || rbtCliente.Checked)
                    {
                        txtTotalHoras.Text = String.Concat(Convert.ToString(CalcularTotalHoras(dataTable)), " Minutos");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Erro ao carregar os dados. Verificar se ID � v�lido.");
                }
            }
        }

        /* Adiciona na base de dados o cliente com o nome escrito na textBox
        * localizada na �rea 'Adicionar Cliente' */
        private void btnAdicionarCliente_Click(object sender, EventArgs e)
        {
            try
            {
                cliente.AdicionarCliente(txtAdicionarCliente.Text);
                MessageBox.Show("Cliente adicionado com sucesso!");
                CarregarAutoCompleteCliente();
                PreencherCbxCliente();
                CarregarAutoCompleteFiltro();
            }
            catch (Exception)
            {
                MessageBox.Show("Erro. Cliente n�o adicionado.");
            }
        }

        /* Remove da base de dados o cliente selecionado da comboBox 
        * localizada na �rea 'Remover Cliente' */
        private void btnRemoverCliente_Click(object sender, EventArgs e)
        {
            string idCliente = cbxNomesClientes.SelectedValue.ToString();

            if (processo.ContarProcessos(idCliente, "cliente") > 0)
            {
                DialogResult result = MessageBox.Show("Este cliente possui registros, deseja mesmo apag�-lo?", "Isto apagar� os registos deste cliente.", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        processo.RemoverProcessos(idCliente, "cliente");
                        cliente.RemoverCliente(idCliente);
                        PreencherCbxCliente();
                        MessageBox.Show("Cliente removido com sucesso!");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Erro. Cliente n�o removido.");
                    }

                }
                else
                {
                    MessageBox.Show("Opera��o cancelada!");
                }
            }
            else
            {
                try
                {
                    cliente.RemoverCliente(idCliente);
                    MessageBox.Show("Cliente removido com sucesso!");
                    CarregarAutoCompleteCliente();
                    PreencherCbxCliente();
                    CarregarAutoCompleteFiltro();
                }
                catch (Exception)
                {
                    MessageBox.Show("Erro. Cliente n�o removido.");
                }
            }
        }

        /* Adiciona na base de dados o funcion�rio com o nome escrito na textBox
         * localizada na �rea 'Adicionar Funcion�rio' */
        private void btnAdicionarFuncionario_Click(object sender, EventArgs e)
        {
            try
            {
                funcionario.AdicionarFuncionario(txtAdicionarFuncionario.Text);
                MessageBox.Show("Funcionario adicionado com sucesso!");
                CarregarAutoCompleteFuncionario();
                PreencherCbxFuncionario();
                CarregarAutoCompleteFiltro();
            }
            catch (Exception)
            {
                MessageBox.Show("Erro. Funcionario n�o adicionado.");
            }
        }

        /* Remove da base de dados o funcion�rio selecionado da comboBox 
         * localizada na �rea 'Remover Funcion�rio' */
        private void btnRemoverFuncionario_Click(object sender, EventArgs e)
        {
            string idFuncionario = cbxNomesFuncionarios.SelectedValue.ToString();

            if (processo.ContarProcessos(idFuncionario, "funcionario") > 0)
            {
                DialogResult result = MessageBox.Show("Este funcion�rio possui registros, deseja mesmo apag�-lo?", "Isto apagar� os registos deste Funcion�rio.", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        processo.RemoverProcessos(idFuncionario, "funcionario");
                        funcionario.RemoverFuncionario(idFuncionario);
                        PreencherCbxFuncionario();
                        MessageBox.Show("Funcion�rio removido com sucesso!");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Erro. Funcion�rio n�o removido.");
                    }

                }
                else
                {
                    MessageBox.Show("Opera��o cancelada!");
                }
            }
            else
            {
                try
                {
                    funcionario.RemoverFuncionario(idFuncionario);
                    MessageBox.Show("Funcion�rio removido com sucesso!");
                    CarregarAutoCompleteFuncionario();
                    PreencherCbxFuncionario();
                    CarregarAutoCompleteFiltro();
                }
                catch (Exception)
                {
                    MessageBox.Show("Erro. Funcion�rio n�o removido.");
                }
            }
        }

        /* Adiciona na base de dados, a categoria com o nome escrito na textBox
        * localizada na �rea 'Adicionar Categoria' */
        private void btnAdicionarCategoria_Click(object sender, EventArgs e)
        {
            try
            {
                categoria.AdicionarCategoria(txtAdicionarCategoria.Text);
                MessageBox.Show("Categoria adicionada com sucesso!");
                PreencherCbxRemoverCategoria();
                PreencherCbxCategoriaAddRegistos();
            }
            catch (Exception)
            {
                MessageBox.Show("Erro. Categoria n�o adicionada.");
            }
        }
       
        private void btnRemoverCategoria_Click(object sender, EventArgs e)
        {
            string idCategoria = cbxNomesCategorias.SelectedValue.ToString();

            if (processo.ContarProcessos(idCategoria, "categoria") > 0)
            {
                DialogResult result = MessageBox.Show("Esta categoria possui registros, deseja mesmo apag�-la?", "Isto apagar� os registos deste categoria.", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        processo.RemoverProcessos(idCategoria, "categoria");
                        categoria.RemoverCategoria(idCategoria);
                        PreencherCbxRemoverCategoria();
                        PreencherCbxCategoriaAddRegistos();
                        MessageBox.Show("Categoria removida com sucesso!");
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Erro. Categoria n�o removida.");
                    }

                }
                else
                {
                    MessageBox.Show("Opera��o cancelada!");
                }
            }
            else
            {
                try
                {
                    categoria.RemoverCategoria(idCategoria);
                    PreencherCbxRemoverCategoria();
                    PreencherCbxCategoriaAddRegistos();
                    MessageBox.Show("Categoria removida com sucesso!");
                }
                catch (Exception)
                {
                    MessageBox.Show("Erro. Categoria n�o removida.");
                }
            }
        }

        /* Soma o total de minutos trabalhados que ser�o exibidos 
         * na caixa de texto 'Total' ap�s filtrar dados dos processos */
        private int CalcularTotalHoras(DataTable dataTable)
        {
            int total = 0;
            string minutos;

            foreach (DataRow row in dataTable.Rows)
            {
                minutos = row[2].ToString().Substring(0, row[2].ToString().IndexOf(" "));
                total += Convert.ToInt16(minutos);
            }

            return total;
        }

        /* Ao clicar no bot�o 'Fazer Backup' 
         * cria um backup da base de dados.*/
        private void btnFazerBackUp_Click(object sender, EventArgs e)
        {
            try
            {
                database.FazerBackup(txtBackup.Text);
                MessageBox.Show("Backup efetuado com sucesso!");
            }
            catch (Exception)
            {
                MessageBox.Show("Erro. Backup falhou!.");
            }
        }

        /* Carrega a comboBox de categorias da �rea 'Registrar novo processo' 
         * com nomes de todas as categorias encontradas na base de dados */
        private void PreencherCbxCategoriaAddRegistos()
        {
            cbxCategoria.ValueMember = "Id";
            cbxCategoria.DisplayMember = "Nome";

            try
            {
                cbxCategoria.DataSource = categoria.BuscarCategorias();
            }
            catch (Exception)
            {
                MessageBox.Show("Erro. A comboBox 'Categorias' n�o foi inicializada.");
            }
        }

        private void PreencherCbxFuncionario()
        {
            cbxNomesFuncionarios.ValueMember = "Id";
            cbxNomesFuncionarios.DisplayMember = "Nome";

            try
            {
                cbxNomesFuncionarios.DataSource = funcionario.BuscarFuncionarios();
            }
            catch (Exception)
            {
                MessageBox.Show("Erro. A comboBox 'Remover Funcionarios' n�o foi inicializada.");
            }
        }

        private void PreencherCbxCliente()
        {
            cbxNomesClientes.ValueMember = "Id";
            cbxNomesClientes.DisplayMember = "Nome";

            try
            {
                cbxNomesClientes.DataSource = cliente.BuscarClientes();
            }
            catch (Exception)
            {
                MessageBox.Show("Erro. A comboBox 'Remover Clientes' n�o foi inicializada.");
            }
        }

        private void PreencherCbxRemoverCategoria()
        {
            cbxNomesCategorias.ValueMember = "Id";
            cbxNomesCategorias.DisplayMember = "Nome";

            try
            {
                cbxNomesCategorias.DataSource = categoria.BuscarCategorias();
            }
            catch (Exception)
            {
                MessageBox.Show("Erro. A comboBox 'Remover Categorias' n�o foi inicializada.");
            }
        }

        /* Carrega a comboBox de ID-Nome dos funcion�rios/clientes da �rea 'Agrupar' em 'Visualizar processos'
         * com ID-Nome de todos os clientes ou funcionarios encontrados na base de dados */
        private void CarregarAutoCompleteFiltro()
        {
            DataTable dataTable = new DataTable();

            string concatString;

            if (rbtNaoAgrupar.Checked == false)
            {
                cbxFiltro.Enabled = true;

                if (rbtCliente.Checked)
                {
                    dataTable = cliente.BuscarClientes();
                }
                else
                {
                    dataTable = funcionario.BuscarFuncionarios();
                }

                cbxFiltro.Items.Clear();

                foreach (DataRow row in dataTable.Rows)
                {
                    concatString = row["Id"].ToString() + " - " + row["Nome"].ToString();
                    cbxFiltro.Items.Add(concatString);
                }
            }
            else
            {
                cbxFiltro.AutoCompleteMode = AutoCompleteMode.None;
                cbxFiltro.AutoCompleteCustomSource = null;
                cbxFiltro.Enabled = false;
            }
        }

        /* Carrega a textBox de ID Cliente da �rea 'Registrar novo processo'
         * com dados de todos os clientes encontrados na base de dados */
        private void CarregarAutoCompleteCliente()
        {
            DataTable datatable = new DataTable();
            datatable = cliente.BuscarClientes();

            string concatString;
            int contagemLinhas = datatable.Rows.Count;
            string[] nameArray = new string[contagemLinhas];

            int i = 0;
            foreach (DataRow row in datatable.Rows)
            {
                concatString = row["Id"].ToString() + " - " + row["Nome"].ToString();
                nameArray[i] = concatString;
                i++;
            }

            txtAutoCliente.Values = nameArray;
        }

        /* Carrega a textBox de ID Funcionario da �rea 'Registrar novo processo' 
         * com dados de todos os funcionarios encontrados na base de dados */
        private void CarregarAutoCompleteFuncionario()
        {
            DataTable datatable = new DataTable();
            datatable = funcionario.BuscarFuncionarios();

            string concatString;
            int contagemLinhas = datatable.Rows.Count;

            string[] nameArray = new string[contagemLinhas];

            int i = 0;
            foreach (DataRow row in datatable.Rows)
            {
                concatString = row["Id"].ToString() + " - " + row["Nome"].ToString();
                nameArray[i] = concatString;
                i++;
            }

            txtAutoFuncionario.Values = nameArray;
        }

        /* M�todos abaixo, configura o que est� sendo exibido na combobox 'ID Funcion�rio/Cliente
         ao clicar nas radioButtons Funcionario, Cliente ou N�o Filtrar. */
        private void rbtFuncionario_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtFuncionario.Checked == true)
            {
                cbxFiltro.Text = "ID Funcionario";
            }
            CarregarAutoCompleteFiltro();
        }

        private void rbtCliente_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtCliente.Checked == true)
            {
                cbxFiltro.Text = "ID Cliente";
            }
            CarregarAutoCompleteFiltro();
        }

        private void rbtNaoFiltrar_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtNaoAgrupar.Checked == true)
            {
                cbxFiltro.Text = "ID Funcionario/Cliente";
            }
            CarregarAutoCompleteFiltro();
        }

        /* M�todos abaixo, conjunto de eventos que configura o que exibir
         e o que ser� oculto ao clicar nas abas. */
        private void btnVisualizarProcessos_Click(object sender, EventArgs e)
        {
            panelRegistrarProcesso.Hide();
            panelAdministracao.Hide();
            panelVisualizarProcessos.Show();
        }

        private void btnRegistrarNovoProcesso_Click(object sender, EventArgs e)
        {
            panelVisualizarProcessos.Hide();
            panelAdministracao.Hide();
            panelRegistrarProcesso.Show();
        }

        private void btnAdmin_Click(object sender, EventArgs e)
        {
            panelVisualizarProcessos.Hide();
            panelRegistrarProcesso.Hide();
            panelAdministracao.Show();
        }

    }
}
