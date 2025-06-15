## VSCodeでの操作方法について  <!--#を1〜6個置くとサイズが大から小の見出し-->
# 作業開始前  
git checkout main (mainブランチに移動する)  
git pull origin main (mainブランチの最新状態を取り込む)  
git checkout -b 自分のブランチ名 (自分のブランチに移動する *ただし、-bは作成時のみ)  
# 作業後  
git add .  
git commit -m "変更内容"  
git push origin 自分のブランチ名  
# push後
Github上のPull requestsでPull Requestを作成  
レビューとmerge (mainブランチに取り込む)  
<!--半角スペース2つで改行-->
